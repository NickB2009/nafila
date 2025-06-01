"""
This script directly updates the Barbearia model files to fix the __str__ method
"""

import os
import re
import shutil
import tempfile

def update_file(file_path, target_pattern, replacement):
    """Update a file by replacing text that matches the target pattern"""
    print(f"Updating file: {file_path}")
    
    # Create a temporary file
    temp_file = tempfile.NamedTemporaryFile(delete=False)
    
    with open(file_path, 'r', encoding='utf-8') as input_file, open(temp_file.name, 'w', encoding='utf-8') as output_file:
        for line in input_file:
            if line.strip() == 'def __str__(self):':
                # Found the start of the __str__ method
                output_file.write(line)  # Write the method signature
                
                # Read the next line which should be the return statement
                next_line = next(input_file)
                
                # Skip the return line and write the replacement
                output_file.write(replacement)
            else:
                output_file.write(line)
    
    # Replace the original file with the updated file
    shutil.move(temp_file.name, file_path)
    print(f"Successfully updated {file_path}")

def main():
    # The files to update
    files = [
        r'c:\git\eutonafila\nafila\eutonafila\barbershop\models.py',
        r'c:\git\eutonafila\nafila\eutonafila\deploy_package\barbershop\models.py'
    ]
    
    # The pattern to match (the __str__ method)
    target_pattern = r'def __str__\(self\):\s*return'
    
    # The replacement text (indented with 8 spaces for proper Python indentation)
    replacement = """        try:
            # Format UUID as string correctly
            id_str = str(self.id) if self.id is not None else "None"
            return f"{self.nome} ({id_str})"
        except Exception as e:
            # Fallback in case of any error
            import logging
            logger = logging.getLogger(__name__)
            logger.error(f"Error in __str__: {str(e)}")
            return self.nome
"""
    
    # Update each file
    for file in files:
        if os.path.exists(file):
            update_file(file, target_pattern, replacement)
        else:
            print(f"File not found: {file}")

if __name__ == "__main__":
    print("Starting to update Barbearia model files...")
    main()
    print("Update complete. Please restart your application for changes to take effect.")
