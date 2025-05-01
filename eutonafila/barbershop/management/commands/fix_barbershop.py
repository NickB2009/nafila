from django.core.management.base import BaseCommand
from django.utils import timezone
from domain.models import Barbearia

class Command(BaseCommand):
    help = 'Fix the barbershop settings to ensure it is open'
    
    def handle(self, *args, **options):
        # Get all barbershops
        barbershops = Barbearia.objects.all()
        if not barbershops:
            self.stdout.write(self.style.ERROR("No barbershops found in database"))
            return
        
        fixed_count = 0
        for barbearia in barbershops:
            changes = []
            
            # Set working days to all days of the week if not already set
            if not barbearia.dias_funcionamento:
                barbearia.dias_funcionamento = list(range(7))
                changes.append("working days")
            
            # Add current day to working days if not already included
            current_day = timezone.now().weekday()
            if current_day not in barbearia.dias_funcionamento:
                barbearia.dias_funcionamento.append(current_day)
                changes.append(f"added current day ({current_day})")
            
            # Update capacity if needed
            if barbearia.max_capacity < 20:
                barbearia.max_capacity = 20
                changes.append("increased capacity")
            
            # Enable priority queue
            if not barbearia.enable_priority_queue:
                barbearia.enable_priority_queue = True
                changes.append("enabled priority queue")
            
            # Save changes
            if changes:
                barbearia.save()
                fixed_count += 1
                self.stdout.write(
                    self.style.SUCCESS(
                        f"Fixed '{barbearia.nome}': {', '.join(changes)}"
                    )
                )
            else:
                self.stdout.write(f"Barbershop '{barbearia.nome}' already has valid settings")
        
        # Summary
        self.stdout.write(
            self.style.SUCCESS(f"Fixed {fixed_count} of {barbershops.count()} barbershops")
        )
        
        # Show status after fixes
        for barbearia in barbershops:
            is_open = barbearia.esta_aberto()
            status = self.style.SUCCESS("OPEN") if is_open else self.style.ERROR("CLOSED")
            self.stdout.write(f"{barbearia.nome}: {status}")
            self.stdout.write(f"  Days: {barbearia.dias_funcionamento}")
            self.stdout.write(f"  Current day: {timezone.now().weekday()}")
            self.stdout.write(f"  Hours: {barbearia.horario_abertura} - {barbearia.horario_fechamento}")
            self.stdout.write(f"  Current time: {timezone.now().time()}")
            self.stdout.write("") 