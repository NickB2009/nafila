# Generated by Django 5.0.2 on 2025-04-25 14:41

from django.db import migrations


class Migration(migrations.Migration):

    dependencies = [
        ('domain', '0004_add_initial_website_builder_data'),
    ]

    operations = [
        migrations.AlterModelOptions(
            name='entradafila',
            options={'ordering': ['created_at']},
        ),
    ]
