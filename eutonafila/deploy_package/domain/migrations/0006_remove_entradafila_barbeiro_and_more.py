# Generated by Django 5.0.2 on 2025-05-03 03:57

import django.utils.timezone
from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('domain', '0005_alter_entradafila_options'),
    ]

    operations = [
        migrations.RemoveField(
            model_name='entradafila',
            name='barbeiro',
        ),
        migrations.RemoveField(
            model_name='entradafila',
            name='cliente',
        ),
        migrations.RemoveField(
            model_name='entradafila',
            name='barbearia',
        ),
        migrations.RemoveField(
            model_name='entradafila',
            name='servico',
        ),
        migrations.RemoveField(
            model_name='servico',
            name='barbearia',
        ),
        migrations.AlterModelOptions(
            name='barbearia',
            options={'verbose_name': 'Barbearia', 'verbose_name_plural': 'Barbearias'},
        ),
        migrations.AlterModelOptions(
            name='pagesection',
            options={'ordering': ['order_index']},
        ),
        migrations.AlterModelOptions(
            name='testimonial',
            options={'ordering': ['-created_at']},
        ),
        migrations.RenameField(
            model_name='testimonial',
            old_name='author_name',
            new_name='customer_name',
        ),
        migrations.RemoveField(
            model_name='barbearia',
            name='cores',
        ),
        migrations.RemoveField(
            model_name='barbearia',
            name='logo',
        ),
        migrations.RemoveField(
            model_name='galleryimage',
            name='description',
        ),
        migrations.RemoveField(
            model_name='galleryimage',
            name='title',
        ),
        migrations.RemoveField(
            model_name='testimonial',
            name='author_image',
        ),
        migrations.AddField(
            model_name='galleryimage',
            name='caption',
            field=models.CharField(blank=True, max_length=200, null=True),
        ),
        migrations.AddField(
            model_name='galleryimage',
            name='created_at',
            field=models.DateTimeField(auto_now_add=True, default=django.utils.timezone.now),
            preserve_default=False,
        ),
        migrations.AddField(
            model_name='pagelayout',
            name='updated_at',
            field=models.DateTimeField(auto_now=True),
        ),
        migrations.AddField(
            model_name='pagesection',
            name='created_at',
            field=models.DateTimeField(auto_now_add=True, default=django.utils.timezone.now),
            preserve_default=False,
        ),
        migrations.AddField(
            model_name='pagesection',
            name='updated_at',
            field=models.DateTimeField(auto_now=True),
        ),
        migrations.AlterField(
            model_name='barbearia',
            name='horario_abertura',
            field=models.TimeField(),
        ),
        migrations.AlterField(
            model_name='barbearia',
            name='horario_fechamento',
            field=models.TimeField(),
        ),
        migrations.AlterField(
            model_name='barbearia',
            name='id',
            field=models.BigAutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID'),
        ),
        migrations.AlterField(
            model_name='barbearia',
            name='max_capacity',
            field=models.IntegerField(default=10),
        ),
        migrations.AlterField(
            model_name='pagesection',
            name='section_type',
            field=models.CharField(max_length=50),
        ),
        migrations.AlterField(
            model_name='testimonial',
            name='rating',
            field=models.IntegerField(choices=[(1, 1), (2, 2), (3, 3), (4, 4), (5, 5)]),
        ),
        migrations.DeleteModel(
            name='Barbeiro',
        ),
        migrations.DeleteModel(
            name='Cliente',
        ),
        migrations.DeleteModel(
            name='EntradaFila',
        ),
        migrations.DeleteModel(
            name='Servico',
        ),
    ]
