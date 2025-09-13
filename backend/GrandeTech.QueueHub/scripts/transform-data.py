#!/usr/bin/env python3
"""
Data Transformation Script
Phase 6.2: Transform SQL Server data for MySQL migration

This script transforms exported SQL Server data to be compatible with MySQL.
"""

import pandas as pd
import uuid
import json
import re
from datetime import datetime
import sys
import os

def transform_guid(guid_str):
    """Convert SQL Server GUID format to MySQL format"""
    if pd.isna(guid_str) or guid_str == 'NULL' or guid_str == '':
        return None
    
    try:
        # Remove braces and convert to lowercase
        if isinstance(guid_str, str):
            guid_str = guid_str.strip()
            if guid_str.startswith('{') and guid_str.endswith('}'):
                guid_str = guid_str[1:-1]
            return str(uuid.UUID(guid_str)).lower()
        return str(guid_str)
    except (ValueError, TypeError):
        print(f"Warning: Invalid GUID format: {guid_str}")
        return None

def transform_boolean(value):
    """Convert SQL Server bit to MySQL TINYINT(1)"""
    if pd.isna(value) or value == 'NULL' or value == '':
        return 0
    
    if isinstance(value, str):
        value = value.strip().lower()
        if value in ['1', 'true', 'yes']:
            return 1
        elif value in ['0', 'false', 'no']:
            return 0
    
    return int(bool(value))

def transform_datetime(dt_str):
    """Convert SQL Server datetime to MySQL DATETIME(6) format"""
    if pd.isna(dt_str) or dt_str == 'NULL' or dt_str == '':
        return None
    
    try:
        if isinstance(dt_str, str):
            # Handle various datetime formats
            dt_str = dt_str.strip()
            if dt_str.endswith('Z'):
                dt_str = dt_str[:-1] + '+00:00'
            
            # Parse and format for MySQL
            dt = pd.to_datetime(dt_str)
            return dt.strftime('%Y-%m-%d %H:%M:%S.%f')[:26]  # Truncate to 6 decimal places
        return dt_str
    except (ValueError, TypeError):
        print(f"Warning: Invalid datetime format: {dt_str}")
        return None

def transform_json(json_str):
    """Transform JSON data for MySQL compatibility"""
    if pd.isna(json_str) or json_str == 'NULL' or json_str == '':
        return None
    
    try:
        if isinstance(json_str, str):
            json_str = json_str.strip()
            if json_str == 'NULL' or json_str == '':
                return None
            
            # Parse and re-serialize to ensure valid JSON
            data = json.loads(json_str)
            return json.dumps(data, separators=(',', ':'))
        return json_str
    except (json.JSONDecodeError, TypeError):
        print(f"Warning: Invalid JSON format: {json_str}")
        return None

def transform_decimal(value):
    """Transform decimal values for MySQL"""
    if pd.isna(value) or value == 'NULL' or value == '':
        return None
    
    try:
        if isinstance(value, str):
            value = value.strip()
            if value == 'NULL' or value == '':
                return None
        return float(value)
    except (ValueError, TypeError):
        print(f"Warning: Invalid decimal format: {value}")
        return None

def process_table(table_name, input_file, output_file):
    """Process a single table's data"""
    print(f"Processing {table_name}...")
    
    try:
        # Read CSV file
        df = pd.read_csv(input_file, na_values=['NULL', ''])
        
        # Transform GUID columns
        guid_columns = ['Id', 'SubscriptionPlanId', 'OrganizationId', 'LocationId', 
                       'CustomerId', 'QueueId', 'StaffMemberId', 'ServiceTypeId']
        
        for col in guid_columns:
            if col in df.columns:
                df[col] = df[col].apply(transform_guid)
        
        # Transform boolean columns
        boolean_columns = ['IsActive', 'IsDeleted', 'IsDefault']
        for col in boolean_columns:
            if col in df.columns:
                df[col] = df[col].apply(transform_boolean)
        
        # Transform datetime columns
        datetime_columns = ['CreatedAt', 'UpdatedAt', 'CalledAt', 'StartedAt', 'CompletedAt']
        for col in datetime_columns:
            if col in df.columns:
                df[col] = df[col].apply(transform_datetime)
        
        # Transform decimal columns
        decimal_columns = ['MonthlyPriceAmount', 'YearlyPriceAmount', 'Price', 'DurationMinutes', 
                          'EstimatedServiceTimeMinutes', 'ActualServiceTimeMinutes']
        for col in decimal_columns:
            if col in df.columns:
                df[col] = df[col].apply(transform_decimal)
        
        # Transform JSON columns
        json_columns = ['BrandingConfig', 'LocationIds', 'StaffMemberIds', 'ServiceTypeIds', 
                       'FavoriteLocationIds', 'SpecialtyServiceTypeIds', 'HaircutDetails']
        for col in json_columns:
            if col in df.columns:
                df[col] = df[col].apply(transform_json)
        
        # Handle string columns - ensure proper encoding
        string_columns = ['Name', 'Description', 'ContactEmail', 'ContactPhone', 'WebsiteUrl', 
                         'Address', 'City', 'State', 'ZipCode', 'PhoneNumber', 'Email', 
                         'FirstName', 'LastName', 'Slug', 'Status', 'Notes']
        for col in string_columns:
            if col in df.columns:
                df[col] = df[col].astype(str).replace('nan', None)
                df[col] = df[col].replace('NULL', None)
        
        # Save transformed data
        df.to_csv(output_file, index=False, na_rep='NULL')
        print(f"✅ {table_name} processed successfully: {len(df)} records")
        
        return len(df)
        
    except Exception as e:
        print(f"❌ Error processing {table_name}: {str(e)}")
        return 0

def main():
    """Main transformation process"""
    print("🔄 Starting SQL Server to MySQL data transformation...")
    
    # Define table mappings
    tables = {
        'subscription_plans': 'SubscriptionPlans',
        'organizations': 'Organizations', 
        'locations': 'Locations',
        'customers': 'Customers',
        'staff_members': 'StaffMembers',
        'service_types': 'ServiceTypes',
        'queues': 'Queues',
        'queue_entries': 'QueueEntries'
    }
    
    # Create output directory
    output_dir = 'transformed_data'
    os.makedirs(output_dir, exist_ok=True)
    
    total_records = 0
    successful_tables = 0
    
    # Process each table
    for file_name, table_name in tables.items():
        input_file = f"{file_name}.csv"
        output_file = f"{output_dir}/{file_name}_transformed.csv"
        
        if os.path.exists(input_file):
            records = process_table(table_name, input_file, output_file)
            total_records += records
            if records > 0:
                successful_tables += 1
        else:
            print(f"⚠️ Input file not found: {input_file}")
    
    # Summary
    print(f"\n📊 Transformation Summary:")
    print(f"✅ Successfully processed: {successful_tables}/{len(tables)} tables")
    print(f"📈 Total records processed: {total_records}")
    print(f"📁 Output directory: {output_dir}")
    
    if successful_tables == len(tables):
        print("🎉 All tables processed successfully!")
    else:
        print("⚠️ Some tables failed to process. Check the logs above.")
    
    return successful_tables == len(tables)

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
