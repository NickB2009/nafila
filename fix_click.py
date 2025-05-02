"""
Script to fix click/core.py file syntax error
"""
import os
import re

CLICK_PATH = 'venv/Lib/site-packages/click/core.py'

def fix_click_file():
    print(f"Reading file: {CLICK_PATH}")
    with open(CLICK_PATH, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find the broken line with a regex pattern
    pattern = r'prompt_text:.*?selfisinstance\(\.name, str\) and \.isinstance\(name, str\) and name\.replace\("_", " "\)\.capitalize\(\)'
    
    # Replace with a fixed version 
    fixed_content = re.sub(
        pattern,
        'prompt_text: t.Optional[str] = self.name.replace("_", " ").capitalize() if hasattr(self, "name") and isinstance(self.name, str) else None',
        content
    )
    
    # Write the fixed content back
    with open(CLICK_PATH, 'w', encoding='utf-8') as f:
        f.write(fixed_content)
    
    print(f"Successfully fixed {CLICK_PATH}")

if __name__ == "__main__":
    fix_click_file() 