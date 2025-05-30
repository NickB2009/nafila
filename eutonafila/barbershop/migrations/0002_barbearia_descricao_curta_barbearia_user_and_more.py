# Generated by Django 5.0.2 on 2025-04-25 14:41

import django.db.models.deletion
import uuid
from django.conf import settings
from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('barbershop', '0001_initial'),
        migrations.swappable_dependency(settings.AUTH_USER_MODEL),
    ]

    operations = [
        migrations.AddField(
            model_name='barbearia',
            name='descricao_curta',
            field=models.CharField(blank=True, help_text='Short description for SEO and previews', max_length=200, null=True),
        ),
        migrations.AddField(
            model_name='barbearia',
            name='user',
            field=models.ForeignKey(blank=True, null=True, on_delete=django.db.models.deletion.CASCADE, related_name='barbearias', to=settings.AUTH_USER_MODEL),
        ),
        migrations.AddField(
            model_name='fila',
            name='barbeiro_preferido',
            field=models.ForeignKey(blank=True, null=True, on_delete=django.db.models.deletion.SET_NULL, related_name='clientes_preferenciais', to='barbershop.barbeiro'),
        ),
        migrations.AddField(
            model_name='fila',
            name='prioridade',
            field=models.IntegerField(choices=[(1, 'Normal'), (2, 'Bronze'), (3, 'Prata'), (4, 'Ouro'), (5, 'VIP')], default=1),
        ),
        migrations.AlterField(
            model_name='barbeiro',
            name='status',
            field=models.CharField(choices=[('available', 'Disponível'), ('busy', 'Ocupado'), ('break', 'Em pausa'), ('offline', 'Ausente')], default='available', max_length=20),
        ),
        migrations.AlterField(
            model_name='fila',
            name='barbeiro',
            field=models.ForeignKey(blank=True, null=True, on_delete=django.db.models.deletion.CASCADE, related_name='atendimentos', to='barbershop.barbeiro'),
        ),
        migrations.AlterField(
            model_name='fila',
            name='id',
            field=models.UUIDField(default=uuid.uuid4, editable=False, primary_key=True, serialize=False),
        ),
        migrations.AlterField(
            model_name='fila',
            name='status',
            field=models.CharField(choices=[('waiting', 'Aguardando'), ('in_service', 'Em Atendimento'), ('completed', 'Finalizado'), ('cancelled', 'Cancelado'), ('no_show', 'Ausente')], default='waiting', max_length=20),
        ),
    ]
