import os
import re

def fix_domain_models():
    """Fix the specific file with the replace() error"""
    file_path = os.path.join("eutonafila", "domain", "domain_models.py")
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Fix the choices methods in Status classes
        pattern1 = r'def choices\(cls\):\s+return \[\(status\.value, status\.name\.replace\(\'STATUS_\', \'\'\)\.title\(\)\)'
        replacement1 = "def choices(cls):\n        return [(status.value, status.name.title() if isinstance(status.name, str) else str(status.name))"
        
        content = re.sub(pattern1, replacement1, content)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        
        print(f"Fixed {file_path}")
        return True
    except Exception as e:
        print(f"Error fixing {file_path}: {e}")
        return False

def fix_entities():
    """Fix entities.py file with the replace() error"""
    file_path = os.path.join("eutonafila", "domain", "entities.py")
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Fix all choices methods in Status classes
        pattern = r'def choices\(cls\):\s+return \[\(status\.value, status\.name\.title\(\)\)'
        replacement = "def choices(cls):\n        return [(status.value, status.name.title() if isinstance(status.name, str) else str(status.name))"
        
        content = re.sub(pattern, replacement, content)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        
        print(f"Fixed {file_path}")
        return True
    except Exception as e:
        print(f"Error fixing {file_path}: {e}")
        return False

if __name__ == "__main__":
    print("Starting targeted fixes...")
    fix_domain_models()
    fix_entities()
    print("Done!") 