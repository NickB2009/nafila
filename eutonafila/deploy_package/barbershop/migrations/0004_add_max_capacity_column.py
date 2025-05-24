from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ('barbershop', '0003_add_max_capacity_and_priority'),
    ]

    operations = [
        migrations.AddField(
            model_name='barbearia',
            name='max_capacity',
            field=models.IntegerField(default=10),
        ),
    ] 