import os
import requests
from pathlib import Path

# URLs for default images
DEFAULT_BANNER_URL = "https://images.unsplash.com/photo-1585747860715-2ba37e788b70?q=80&w=2074&auto=format&fit=crop"
DEFAULT_LOGO_URL = "https://img.freepik.com/premium-vector/barbershop-vintage-logo_441059-145.jpg?w=360"

def download_file(url, save_path):
    """Download a file from URL and save it to the specified path"""
    response = requests.get(url, stream=True)
    response.raise_for_status()  # Raise an error for bad responses
    
    # Create directory if it doesn't exist
    os.makedirs(os.path.dirname(save_path), exist_ok=True)
    
    with open(save_path, 'wb') as f:
        for chunk in response.iter_content(chunk_size=8192):
            f.write(chunk)
    
    print(f"Downloaded: {save_path}")

def download_resources():
    """Download necessary resources for the project"""
    # Get the staticfiles directory
    static_dir = Path('static')
    img_dir = static_dir / 'img'
    
    # Create directories if they don't exist
    os.makedirs(img_dir, exist_ok=True)
    
    # Download the default banner image
    banner_path = img_dir / 'barbershop-default-banner.jpg'
    if not os.path.exists(banner_path):
        download_file(DEFAULT_BANNER_URL, banner_path)
    else:
        print(f"Banner already exists: {banner_path}")
    
    # Download the default logo
    logo_path = img_dir / 'logo-footer.png'
    if not os.path.exists(logo_path):
        download_file(DEFAULT_LOGO_URL, logo_path)
    else:
        print(f"Logo already exists: {logo_path}")

if __name__ == "__main__":
    print("Downloading resources...")
    download_resources()
    print("Download complete.") 