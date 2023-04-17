import requests
from bs4 import BeautifulSoup
import pandas as pd
import re

# Flag
def get_flag():
    pattern = r"Flag of (.+)" # 正規表現パターン

    def is_flag_tag(x):
        try:
            match = re.search(pattern, x.find("img").get("alt"))
            if match:
                return True
            else:
                return False
        except:
            return False
        
    res = requests.get('https://flagpedia.net/index')
    # li_tags = soup.find_all("li", class_="mf dt dl")
    soup = BeautifulSoup(res.text, "html.parser")
    li_tags = soup.find_all("li")
    li_tags = [s for s in li_tags if is_flag_tag(s)]
    img_urls = [s.find("img").get("src") for s in li_tags]
    nation_names = [s.find("span").text for s in li_tags]
    df = pd.DataFrame()
    df["img_url"] = img_urls
    df["nation_name"] = nation_names
    df.loc[:,"img_url"] = "https://flagpedia.net/" + df["img_url"]
    df.to_csv('./nation_flag.csv', index=False)
    return df

# mp3
def get_nationalanthem_mp3():
    headers = {
        'user-agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36',
    }
    res = requests.get('https://nationalanthems.info', headers=headers)
    soup = BeautifulSoup(res.text, "html.parser")
    sub_menus = soup.find_all("ul", class_="sub-menu")
    data = []
    for sub_menu in sub_menus:
        data.extend(sub_menu.find_all("a", class_="menu-link"))

    def get_href(s):
        try:
            return s.get('href')
        except:
            return None

    nationalanthems_df = pd.DataFrame()
    nationalanthems_df["nation_name"] = [s.find("span").text for s in data]
    nationalanthems_df["page_url"] = [get_href(s) for s in data]
    nation_flag_org = pd.read_csv('./nation_flag.csv')
    nationalanthems_org = nationalanthems_df.set_index("nation_name")["page_url"].to_dict()
    nationalanthems = dict()
    flag = True
    nationalanthems_use_nations =  [nation_name for nation_name in nationalanthems_org.keys() if nation_name in nation_flag_org["nation_name"].values]
    data = []
    nation_data_names = []
    for i, row in nationalanthems_df.iterrows():
        if (re.search(r'\d+' , row["nation_name"])):
            data[-1].append(row["page_url"])
        else:
            data.append([row["page_url"]])
            nation_data_names.append(row["nation_name"])
    page_urls = [s[-1] for s in data]
    new_nationalanthems_df = pd.DataFrame()
    new_nationalanthems_df["nation_name"] = nation_data_names
    new_nationalanthems_df["page_url"] = page_urls
    return new_nationalanthems_df

def get_locations():
    res = requests.get("https://developers.google.com/public-data/docs/canonical/countries_csv?hl=ja")
    soup = BeautifulSoup(res.text, "html.parser")
    # ['country', 'latitude', 'longitude', 'name']
    loc_data = []
    for tr_tag in  soup.find('div', class_="devsite-article-body clearfix").find_all("tr"):
        loc_data.append([s.text for s in tr_tag.find_all("td")])
    loc_ja_df = pd.DataFrame(loc_data[1:], columns=['country', 'latitude', 'longitude', 'name'])

    res = requests.get("https://developers.google.com/public-data/docs/canonical/countries_csv")
    soup = BeautifulSoup(res.text, "html.parser")
    # soup
    # ['country', 'latitude', 'longitude', 'name']
    loc_data = []
    for tr_tag in  soup.find('div', class_="devsite-article-body clearfix").find_all("tr"):
        loc_data.append([s.text for s in tr_tag.find_all("td")])
    loc_df = pd.DataFrame(loc_data[1:], columns=['country', 'latitude', 'longitude', 'name'])
    return loc_ja_df, loc_df

def main():
    flag_df = get_flag()
    new_nationalanthems_df = get_nationalanthem_mp3()
    new_nationalanthems_dict = new_nationalanthems_df.set_index("nation_name")["page_url"].to_dict()

    flag_df["nationalanthems_pages"] = flag_df["nation_name"].map(new_nationalanthems_dict)
    flag_df = flag_df.dropna(axis=0).reset_index(drop=True)
    flag_df = flag_df[flag_df["nationalanthems_pages"].apply(lambda x: x[-4:]==".htm")]
    flag_df["nationalanthems_mp3"] = flag_df["nationalanthems_pages"].apply(lambda x:x[:-4] + ".mp3")
    nation_flag_anthems = flag_df.reset_index(drop=True)
    flag_df.to_csv('./nation_flag_anthems.csv', index=False)

    loc_ja_df, loc_df = get_locations()
    nation_flag_anthems_loc = nation_flag_anthems.merge(right=loc_df, right_on="name", left_on="nation_name").drop(columns="name")
    nation_flag_anthems_loc.to_csv('./nation_flag_anthems_loc.csv', index=False)
