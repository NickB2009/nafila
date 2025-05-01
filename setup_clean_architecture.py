import os
import sys

def create_directory_structure(base_path):
    """Create the directory structure for Clean Architecture"""
    # Define the directory structure
    structure = {
        'domain': [
            'entities',
            'value_objects',
            'services',
            'exceptions',
            'repositories'
        ],
        'application': [
            'use_cases/queue',
            'use_cases/barbershop',
            'use_cases/client',
            'interfaces/services',
            'dtos',
            'exceptions'
        ],
        'infrastructure': [
            'repositories/django',
            'repositories/memory',
            'orm',
            'services',
            'cache',
            'messaging'
        ],
        'interfaces': [
            'api/rest/serializers',
            'api/rest/views',
            'api/graphql',
            'web/views'
        ]
    }
    
    # Create directories and __init__.py files
    for parent_dir, subdirs in structure.items():
        parent_path = os.path.join(base_path, parent_dir)
        
        # Create parent directory if it doesn't exist
        os.makedirs(parent_path, exist_ok=True)
        
        # Create __init__.py in parent directory
        init_file = os.path.join(parent_path, '__init__.py')
        if not os.path.exists(init_file):
            open(init_file, 'w').close()
        
        # Create subdirectories and their __init__.py files
        for subdir in subdirs:
            subdir_path = os.path.join(parent_path, subdir)
            os.makedirs(subdir_path, exist_ok=True)
            
            # Create __init__.py in each subdirectory
            for path_part in subdir.split('/'):
                current_path = os.path.join(parent_path, path_part)
                init_file = os.path.join(current_path, '__init__.py')
                if not os.path.exists(init_file):
                    open(init_file, 'w').close()
    
    print(f"Clean Architecture directory structure created in {base_path}")

def main():
    """Main function"""
    # Use the current directory as the base path
    base_path = os.getcwd()
    create_directory_structure(base_path)

if __name__ == "__main__":
    main() 