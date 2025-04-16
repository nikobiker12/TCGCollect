import json
import requests
import os
import re
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse, parse_qs, urlunparse

def get_cards(text, lang, set_code):
    # Parse the HTML
    soup = BeautifulSoup(text, "html.parser")

    cards = []

    # Find all card blocks inside <dl class="modalCol">
    for dl in soup.find_all("dl", class_="modalCol"):
        card = {}
        # Get card id from the <dl> attribute
        card['id'] = dl.get("id", "").strip()

        # Extract header information from <dt>
        dt = dl.find("dt")
        if dt:
            # Extract the infoCol values:
            info_col = dt.find("div", class_="infoCol")
            if info_col:
                spans = info_col.find_all("span")
                if len(spans) >= 3:
                    card['code'] = spans[0].get_text(strip=True)
                    card['rarity'] = spans[1].get_text(strip=True)
                    card['role'] = spans[2].get_text(strip=True)
            # Get the card name:
            card_name_div = dt.find("div", class_="cardName")
            if card_name_div:
                card['name'] = card_name_div.get_text(strip=True)

        # Extract detailed information from <dd>
        dd = dl.find("dd")
        if dd:
            # Front part: Get the card image and alt text
            front_col = dd.find("div", class_="frontCol")
            if front_col:
                img = front_col.find("img", class_="lazy")
                if img:
                    card['image'] = img.get("data-src", img.get("src"))
                    card['image_alt'] = img.get("alt", "").strip()
            
            # Back part: Get various card details
            back_col = dd.find("div", class_="backCol")
            if back_col:
                # Utility function: Remove the <h3> header and return the cleaned text.
                def get_detail(div_element):
                    if div_element:
                        header = div_element.find("h3")
                        if header:
                            header.extract()
                        return div_element.get_text(separator=" ", strip=True)
                    return ""
                
                # Extract cost ('Vie')
                cost_div = back_col.find("div", class_="cost")
                card['cost'] = get_detail(cost_div)
                
                # Extract attribute:
                attribute_div = back_col.find("div", class_="attribute")
                if attribute_div:
                    i_tag = attribute_div.find("i")
                    if i_tag:
                        card['attribute'] = i_tag.get_text(strip=True)
                    else:
                        card['attribute'] = get_detail(attribute_div)
                
                # Extract power ('Puissance')
                power_div = back_col.find("div", class_="power")
                card['power'] = get_detail(power_div)
                
                # Extract counter ('Contre')
                counter_div = back_col.find("div", class_="counter")
                card['counter'] = get_detail(counter_div)
                
                # Extract color ('Couleur')
                color_div = back_col.find("div", class_="color")
                card['color'] = get_detail(color_div)
                
                # Extract feature (or type) 
                feature_div = back_col.find("div", class_="feature")
                card['feature'] = get_detail(feature_div)
                
                # Extract effect ('Effet')
                effect_div = back_col.find("div", class_="text")
                card['effect'] = get_detail(effect_div)
                
                # Extract extension info ('Extension')
                extension_div = back_col.find("div", class_="getInfo")
                card['extension'] = get_detail(extension_div)
        
        card['lang'] = lang
        card['set'] = set_code
        cards.append(card)

    return cards

def download_image(base_url, image_url, card_id, target_dir):
    """
    Downloads an image from image_url and saves it to the "images" folder.
    The file is named after the card's id with the appropriate extension.
    """
    # Create a complete URL for the image if it's a relative path
    full_url = urljoin(base_url, image_url)
    # Attempt to extract file extension from URL (default to jpg if not found)
    match = re.search(r'\.(\w+)(?:\?|$)', full_url)
    ext = match.group(1) if match else "jpg"
    filename = os.path.join(target_dir, f"{card_id}.{ext}")
    if os.path.exists(filename):
        print(f"Image for card {card_id} already exists at {filename}. Skipping download.")
        return
    
    try:
        response = requests.get(full_url, stream=True)
        response.raise_for_status()

        # Write the content in chunks
        with open(filename, "wb") as file:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    file.write(chunk)
        print(f"Downloaded image for card {card_id} to {filename}")
    except Exception as e:
        print(f"Failed to download image for card {card_id} from {full_url}: {e}")

def fetch_page_data(url):
    """
    Performs a POST request against the specified URL with the given headers and form payload.
    Returns the HTML text on success.
    """

    # Parse the URL to extract and remove the query parameters
    parsed_url = urlparse(url)
    query_params = parse_qs(parsed_url.query)
    # Extract the "series" parameter; if absent, it defaults to an empty string.
    series_value = query_params.get("series", [""])[0]
    # Remove all query parameters from the URL (i.e. set query to empty)
    cleaned_url = urlunparse(parsed_url._replace(query=""))

    headers = {
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
        "Accept-Encoding": "gzip, deflate, br, zstd",
        "Accept-Language": "fr,fr-FR;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6",
        "Cache-Control": "no-cache",
        "Content-Type": "application/x-www-form-urlencoded",
    }
    
    # Form payload with parameters freewords and series empty
    payload = {
        "freewords": "",
        "series": series_value
    }
    
    try:
        response = requests.post(cleaned_url, headers=headers, data=payload)
        response.raise_for_status()
        print("Page data downloaded successfully.")
        return response.text
    except requests.exceptions.RequestException as e:
        print(f"Error retrieving page data: {e}")
        return ""


json_file = "configuration.json"  # Ensure this file exists in your working directory.
try:
    with open(json_file, "r", encoding="utf-8") as f:
        urls_list = json.load(f)
except Exception as e:
    print(f"Error reading JSON file {json_file}: {e}")
    exit(-1)


target_dir = "C:/dev/opcg-data"

# Process each URL from the configuration file.
for item in urls_list:
    url = item.get("url")
    lang = item.get("lang")
    set_code = item.get("set")  # 'set' is used to denote the card set.
    if not url:
        continue
    
    # Read the HTML content from file
    text = fetch_page_data(url)

    cards = get_cards(text, lang, set_code)

    # Download the image for each card if an image URL is available

    os.makedirs(target_dir, exist_ok=True)
    for card in cards:
        if "image" in card and card["image"]:
            download_image(url, card["image"], card["id"], target_dir)
        
    output_file = f"one_piece_cards_{lang}_{set_code}.json"        
    # Save the collected card data to a JSON file
    with open(target_dir + "/" + output_file, "w", encoding="utf-8") as json_file:
        json.dump(cards, json_file, ensure_ascii=False, indent=4)

    print(f"Extracted {len(cards)} card(s) and saved to {output_file}.")
