"""
Script to permanently fix the UUID issue in the Barbearia model by modifying the model file
"""
import os
import sys
import fileinput
import re
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

logger = logging.getLogger(__name__)

def modify_model_file(file_path):
    """Modify the __str__ method in the model file"""
    try:
        # Patterns to find
        str_method_pattern = r"def __str__\(self\):\s*return f\"{self\.nome} \(\{self\.id\}\)\""
        deployment_str_method_pattern = r"def __str__\(self\):\s*return self\.nome"
        
        # New __str__ method
        new_str_method = """    def __str__(self):
        try:
            # Format UUID as string correctly
            id_str = str(self.id) if self.id is not None else "None"
            return f"{self.nome} ({id_str})"
        except Exception as e:
            # Fallback in case of any error
            return self.nome"""
        
        # Read the file
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Replace the __str__ method
        if re.search(str_method_pattern, content):
            modified_content = re.sub(str_method_pattern, new_str_method, content)
        elif re.search(deployment_str_method_pattern, content):
            modified_content = re.sub(deployment_str_method_pattern, new_str_method, content)
        else:
            logger.warning(f"Could not find __str__ method pattern in {file_path}")
            return False
        
        # Write back to file
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(modified_content)
        
        logger.info(f"Successfully modified {file_path}")
        return True
            
    except Exception as e:
        logger.error(f"Error modifying model file: {str(e)}", exc_info=True)
        return False

def main():
    """Main function to fix all model files"""
    try:
        # Paths to model files
        paths = [
            r"c:\git\eutonafila\nafila\eutonafila\barbershop\models.py",
            r"c:\git\eutonafila\nafila\eutonafila\deploy_package\barbershop\models.py"
        ]
        
        success_count = 0
        
        for path in paths:
            if os.path.exists(path):
                if modify_model_file(path):
                    success_count += 1
            else:
                logger.warning(f"File not found: {path}")
        
        logger.info(f"Modified {success_count} of {len(paths)} files")
        
    except Exception as e:
        logger.error(f"Error in main function: {str(e)}", exc_info=True)

if __name__ == "__main__":
    logger.info("Starting permanent fix for Barbearia __str__ method...")
    main()
