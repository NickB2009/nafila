"""
Script to fix corrupted Django field __init__.py file
"""

import os
import sys

# Path to Django field __init__.py file (absolute path)
django_field_path = os.path.join('C:/', 'repos', 'nafila', 'venv', 'Lib', 'site-packages', 
                               'django', 'db', 'models', 'fields', '__init__.py')

print(f"Looking for Django file at: {django_field_path}")

# Read the file
with open(django_field_path, 'r') as f:
    content = f.read()

# Fix the corrupted line
content = content.replace(
    'self.verbose_name = selfisinstance(.name, str) and .isinstance(name, str) and name.replace("_", " ")',
    'self.verbose_name = self.name and isinstance(self.name, str) and self.name.replace("_", " ")'
)

# Write the fixed content back
with open(django_field_path, 'w') as f:
    f.write(content)

print(f"Successfully fixed Django field __init__.py at {django_field_path}") 