"""
This script will create a permanent fix for the Barbearia model by making a direct edit
to ensure the __str__ method safely handles UUID objects.
"""
import os
import re
import shutil

def backup_file(file_path):
    """Make a backup of the file"""
    backup_path = file_path + '.bak'
    shutil.copy2(file_path, backup_path)
    print(f"Created backup at {backup_path}")

def fix_model_file(file_path):
    """Fix the __str__ method in the model file"""
    # First make a backup
    backup_file(file_path)
    
    # Read the file
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Look for the Barbearia class definition
    barbearia_class_pattern = r'class Barbearia\(models\.Model\):'
    if not re.search(barbearia_class_pattern, content):
        print(f"Could not find Barbearia class in {file_path}")
        return False
    
    # Define the proper __str__ method
    str_method = """    def __str__(self):
        try:
            # Format UUID as string correctly
            id_str = str(self.id) if self.id is not None else "None"
            return f"{self.nome} ({id_str})"
        except Exception as e:
            # Handle any errors
            return self.nome
"""
    
    # Find existing __str__ method in Barbearia class
    str_method_pattern = r'def __str__\(self\):(\s+).*?(\s+)return.*?\n'
    
    # If found, replace it
    if re.search(str_method_pattern, content, re.DOTALL):
        modified_content = re.sub(str_method_pattern, str_method, content, flags=re.DOTALL)
        print("Found and replaced existing __str__ method")
    else:
        # Insert after class definition
        insertion_point = re.search(barbearia_class_pattern, content).end()
        lines = content.splitlines()
        
        # Find where to insert (after class fields)
        for i, line in enumerate(lines):
            if barbearia_class_pattern in line:
                # Find the first method after the class definition
                for j in range(i+1, len(lines)):
                    if 'def ' in lines[j]:
                        insertion_point = j
                        break
                break
        
        # Insert the __str__ method
        lines.insert(insertion_point, str_method)
        modified_content = '\n'.join(lines)
        print("Inserted new __str__ method")
    
    # Write back to file
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(modified_content)
    
    print(f"Successfully updated {file_path}")
    return True

def main():
    # Path to the model files
    model_files = [
        r"c:\git\eutonafila\nafila\eutonafila\barbershop\models.py",
        r"c:\git\eutonafila\nafila\eutonafila\deploy_package\barbershop\models.py"
    ]
    
    for file_path in model_files:
        if os.path.exists(file_path):
            print(f"Processing {file_path}...")
            fix_model_file(file_path)
        else:
            print(f"File not found: {file_path}")

if __name__ == "__main__":
    print("Starting permanent fix for Barbearia __str__ method...")
    main()
    print("Fix complete. The Barbearia model now correctly handles UUID objects in its string representation.")
