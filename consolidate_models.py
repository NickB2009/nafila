"""
This script will help consolidate the duplicate models between domain/models.py and barbershop/models.py.
We're going to keep the domain models as the primary source and update references in barbershop app.

Steps:
1. Analyze both model files to understand differences
2. Create migration to update any necessary foreign keys
3. Remove barbershop/models.py models that duplicate domain models
4. Update imports in other files

Run this script with:
    python manage.py shell < consolidate_models.py
"""

import os
import re
from pathlib import Path

# Define the project root
PROJECT_ROOT = Path(__file__).parent

def analyze_model_files():
    """Compare domain/models.py and barbershop/models.py to identify duplications"""
    domain_models_path = PROJECT_ROOT / "domain" / "models.py"
    barbershop_models_path = PROJECT_ROOT / "barbershop" / "models.py"
    
    if not domain_models_path.exists() or not barbershop_models_path.exists():
        print("Error: Could not find one or both model files")
        return
    
    with open(domain_models_path, 'r') as f:
        domain_content = f.read()
    
    with open(barbershop_models_path, 'r') as f:
        barbershop_content = f.read()
    
    # Extract model class names
    domain_models = re.findall(r'class\s+(\w+)\(models\.Model\):', domain_content)
    barbershop_models = re.findall(r'class\s+(\w+)\(models\.Model\):', barbershop_content)
    
    print("Domain models:", domain_models)
    print("Barbershop models:", barbershop_models)
    
    # Find duplicates
    duplicates = set(domain_models).intersection(set(barbershop_models))
    print("Duplicate models:", duplicates)
    
    return {
        "domain_models": domain_models,
        "barbershop_models": barbershop_models,
        "duplicates": duplicates
    }

def create_migration_plan(analysis):
    """Create a migration plan based on the analysis"""
    if not analysis:
        return
    
    duplicates = analysis["duplicates"]
    migration_plan = []
    
    for model in duplicates:
        migration_plan.append(f"1. Update references to barbershop.{model} to use domain.{model}")
    
    migration_plan.append("2. Create a migration to remove duplicate models from barbershop")
    migration_plan.append("3. Fix any import statements in the codebase")
    
    print("\nMigration Plan:")
    for step in migration_plan:
        print(f" - {step}")

def find_references():
    """Find references to the barbershop models that need to be updated"""
    references = {}
    ignore_dirs = ['__pycache__', 'migrations']
    
    for root, dirs, files in os.walk(PROJECT_ROOT):
        dirs[:] = [d for d in dirs if d not in ignore_dirs]
        
        for file in files:
            if file.endswith('.py'):
                file_path = os.path.join(root, file)
                with open(file_path, 'r') as f:
                    content = f.read()
                    
                    if 'from barbershop.models import' in content:
                        matches = re.findall(r'from\s+barbershop\.models\s+import\s+([^;\n]+)', content)
                        if matches:
                            references[file_path] = matches
    
    print("\nReferences to barbershop.models:")
    for file_path, imports in references.items():
        print(f" - {file_path}: {', '.join(imports)}")
    
    return references

if __name__ == "__main__":
    analysis = analyze_model_files()
    create_migration_plan(analysis)
    references = find_references()
    
    print("\nNext steps:")
    print("1. Run 'python manage.py makemigrations' to create migration for model removal")
    print("2. Update imports in the files mentioned above")
    print("3. Run migrations with 'python manage.py migrate'") 