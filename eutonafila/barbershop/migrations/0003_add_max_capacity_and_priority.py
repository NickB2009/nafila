from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('barbershop', '0002_barbearia_descricao_curta_barbearia_user_and_more'),
    ]

    operations = [
        migrations.AddField(
            model_name='barbearia',
            name='max_capacity',
            field=models.IntegerField(default=10),
        ),
        migrations.AddField(
            model_name='barbearia',
            name='enable_priority_queue',
            field=models.BooleanField(default=False),
        ),
    ] 