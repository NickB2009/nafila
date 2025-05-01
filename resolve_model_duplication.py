"""
This script guides the resolution of model duplication between barbershop.models and domain.models.
"""

import os
import importlib
import inspect
from pathlib import Path
from types import ModuleType
from typing import List, Dict, Set, Tuple

# Configuration
DOMAIN_MODULE = "domain.models"
BARBERSHOP_MODULE = "barbershop.models"
PROJECT_ROOT = Path(__file__).resolve().parent

def analyze_model_duplication():
    """Analyze model duplication between domain.models and barbershop.models"""
    # Load modules
    domain_models = importlib.import_module(DOMAIN_MODULE)
    barbershop_models = importlib.import_module(BARBERSHOP_MODULE)
    
    # Get model classes from each module
    domain_model_classes = get_model_classes(domain_models)
    barbershop_model_classes = get_model_classes(barbershop_models)
    
    # Find duplicated model names
    domain_names = {cls.__name__ for cls in domain_model_classes}
    barbershop_names = {cls.__name__ for cls in barbershop_model_classes}
    duplicate_names = domain_names.intersection(barbershop_names)
    
    print(f"Found {len(duplicate_names)} duplicate model names: {', '.join(sorted(duplicate_names))}")
    
    # Analyze field differences in duplicate models
    duplicate_analysis = []
    for model_name in duplicate_names:
        domain_model = next(cls for cls in domain_model_classes if cls.__name__ == model_name)
        barbershop_model = next(cls for cls in barbershop_model_classes if cls.__name__ == model_name)
        
        differences = compare_model_fields(domain_model, barbershop_model)
        duplicate_analysis.append((model_name, differences))
    
    return {
        "domain_models": domain_model_classes,
        "barbershop_models": barbershop_model_classes,
        "duplicates": duplicate_analysis
    }

def get_model_classes(module: ModuleType) -> List:
    """Extract model classes from a Django module"""
    from django.db.models import Model
    
    model_classes = []
    for name, obj in inspect.getmembers(module):
        if inspect.isclass(obj) and issubclass(obj, Model) and obj.__module__ == module.__name__:
            model_classes.append(obj)
    
    return model_classes

def compare_model_fields(model1, model2) -> Dict:
    """Compare fields between two models"""
    from django.db.models.fields import Field
    
    def get_field_info(model):
        return {
            field.name: {
                "type": type(field).__name__,
                "attrs": {k: v for k, v in field.__dict__.items() 
                          if not k.startswith("_") and k not in ["model", "creation_counter"]}
            } 
            for field in model._meta.fields
        }
    
    fields1 = get_field_info(model1)
    fields2 = get_field_info(model2)
    
    field_names1 = set(fields1.keys())
    field_names2 = set(fields2.keys())
    
    only_in_model1 = field_names1 - field_names2
    only_in_model2 = field_names2 - field_names1
    common_fields = field_names1.intersection(field_names2)
    
    # Check for differences in common fields
    field_differences = {}
    for field_name in common_fields:
        if fields1[field_name]["type"] != fields2[field_name]["type"]:
            if "field_differences" not in field_differences:
                field_differences["field_differences"] = {}
            field_differences["field_differences"][field_name] = {
                "model1_type": fields1[field_name]["type"],
                "model2_type": fields2[field_name]["type"]
            }
    
    return {
        "only_in_model1": list(only_in_model1),
        "only_in_model2": list(only_in_model2),
        "common_fields": list(common_fields),
        "field_differences": field_differences
    }

def create_migration_plan(analysis):
    """Create a migration plan based on analysis results"""
    if not analysis or not analysis.get("duplicates"):
        print("No duplication to resolve.")
        return
    
    print("\nMigration Plan:")
    print("===============")
    
    for model_name, differences in analysis["duplicates"]:
        print(f"\n{model_name}:")
        only_in_domain = differences.get("only_in_model1", [])
        only_in_barbershop = differences.get("only_in_model2", [])
        
        if only_in_domain:
            print(f"  Fields only in domain model: {', '.join(only_in_domain)}")
        
        if only_in_barbershop:
            print(f"  Fields only in barbershop model: {', '.join(only_in_barbershop)}")
            print("  Action: Need to create migration to add these fields to domain model")
        
        field_differences = differences.get("field_differences", {})
        if field_differences:
            print("  Fields with type differences:")
            for field, diff in field_differences.items():
                print(f"    {field}: domain={diff['model1_type']}, barbershop={diff['model2_type']}")
            print("  Action: Need to resolve field type differences")
    
    print("\nGeneral steps:")
    print("1. Create migrations to align field definitions between models")
    print("2. Update imports in all files to use domain models")
    print("3. Remove duplicate model definitions from barbershop.models")
    print("4. Run collectstatic and test the application")

def search_model_imports(model_names: List[str]):
    """Search for imports of duplicate models"""
    import re
    
    model_imports = {}
    for model_name in model_names:
        model_imports[model_name] = {
            "from_domain": [],
            "from_barbershop": []
        }
    
    python_files = []
    for root, dirs, files in os.walk(PROJECT_ROOT):
        if any(ignore in root for ignore in ['__pycache__', 'migrations', 'venv', 'env']):
            continue
        for file in files:
            if file.endswith('.py'):
                python_files.append(os.path.join(root, file))
    
    domain_pattern = re.compile(r'from\s+domain\.models\s+import\s+([^;\n]+)')
    barbershop_pattern = re.compile(r'from\s+barbershop\.models\s+import\s+([^;\n]+)')
    
    for file_path in python_files:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
            
            # Check domain imports
            for match in domain_pattern.finditer(content):
                imports = [name.strip() for name in match.group(1).split(',')]
                for model_name in model_names:
                    if model_name in imports:
                        model_imports[model_name]["from_domain"].append(file_path)
            
            # Check barbershop imports
            for match in barbershop_pattern.finditer(content):
                imports = [name.strip() for name in match.group(1).split(',')]
                for model_name in model_names:
                    if model_name in imports:
                        model_imports[model_name]["from_barbershop"].append(file_path)
    
    return model_imports

def print_import_analysis(import_analysis):
    """Print analysis of model imports"""
    print("\nImport Analysis:")
    print("===============")
    
    for model_name, imports in import_analysis.items():
        domain_count = len(imports["from_domain"])
        barbershop_count = len(imports["from_barbershop"])
        print(f"\n{model_name}:")
        print(f"  Imported from domain.models in {domain_count} files")
        print(f"  Imported from barbershop.models in {barbershop_count} files")
        
        if barbershop_count > 0:
            print("  Files to update:")
            for file_path in imports["from_barbershop"]:
                rel_path = os.path.relpath(file_path, PROJECT_ROOT)
                print(f"    - {rel_path}")

def main():
    print("Analyzing model duplication between domain.models and barbershop.models...")
    analysis = analyze_model_duplication()
    
    # Extract duplicate model names for import analysis
    duplicate_model_names = [item[0] for item in analysis.get("duplicates", [])]
    
    if duplicate_model_names:
        create_migration_plan(analysis)
        
        print("\nSearching for imports of duplicate models...")
        import_analysis = search_model_imports(duplicate_model_names)
        print_import_analysis(import_analysis)
        
        print("\nNext steps:")
        print("1. Run 'python manage.py makemigrations' to prepare migrations")
        print("2. Review the migration files to ensure they match your expectations")
        print("3. Update imports in the files listed above")
        print("4. Remove duplicate models from barbershop/models.py")
        print("5. Run migrations with 'python manage.py migrate'")
    else:
        print("No duplicate models found. Nothing to do.")

if __name__ == "__main__":
    main() 