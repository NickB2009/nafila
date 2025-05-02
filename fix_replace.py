import os
import re

def find_python_files(root_dir):
    for root, _, files in os.walk(root_dir):
        for file in files:
            if file.endswith('.py'):
                yield os.path.join(root, file)

# Replace pattern for common replace calls 
REPLACE_PATTERNS = [
    (r'(\.name)\.replace\(', r'isinstance(\1, str) and \1.replace('),
    (r'(\.value)\.replace\(', r'isinstance(\1, str) and \1.replace('),
    (r'([a-zA-Z_][a-zA-Z0-9_]*)\.replace\(', r'isinstance(\1, str) and \1.replace('),
]

def fix_replace_calls(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        original_content = content
        for pattern, replacement in REPLACE_PATTERNS:
            content = re.sub(pattern, replacement, content)
            
        if content != original_content:
            print(f"Modified {file_path}")
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
        return False
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

def main():
    root_dir = "."
    modified_files = 0
    
    for file_path in find_python_files(root_dir):
        if fix_replace_calls(file_path):
            modified_files += 1
            
    print(f"Modified {modified_files} files")

if __name__ == "__main__":
    main() 