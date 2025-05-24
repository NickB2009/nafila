@echo off
echo ======================================
echo     CORRECAO COMPLETA E STARTUP
echo ======================================
echo.

:: Fix indentation issues first
echo Corrigindo problemas de indentacao...
powershell -Command "$content = Get-Content -Path 'eutonafila\barbershop\management\commands\ensure_services_fixed.py' -Raw; Set-Content -Path 'eutonafila\barbershop\management\commands\ensure_services.py' -Value $content"

:: Fix domain models indentation
echo Corrigindo indentacao em domain\models.py...
powershell -Command "$content = @'
# Domain models module

class EntradaFila:
    STATUS_AGUARDANDO = 1
    STATUS_EM_ATENDIMENTO = 2
    STATUS_ATENDIDO = 3
    STATUS_DESISTIU = 4
    STATUS_ESCOLHEU_ESPERAR_EM_LOJA = 5
    STATUS_ESCOLHEU_ESPERAR_FORA = 6
    
    def get_position(self):
        \"\"\"Get position in queue (FIFO order)\"\"\"
        if self.status != self.STATUS_AGUARDANDO:
            return 0
        
        # Simply use position number (FIFO order)
        return self.position_number 

    def get_priority_position(self):
        \"\"\"Get position in queue, considering priority level\"\"\"
        if self.status != self.STATUS_AGUARDANDO:
            return 0
        
        # Count clients with higher priority
        higher_priority_count = EntradaFila.objects.filter(
            barbearia=self.barbearia,
            status=self.STATUS_AGUARDANDO,
            prioridade__gt=self.prioridade
        ).count()
        
        # Count clients with same priority but earlier arrival time
        same_priority_earlier_arrival = EntradaFila.objects.filter(
            barbearia=self.barbearia,
            status=self.STATUS_AGUARDANDO,
            prioridade=self.prioridade,
            data_entrada__lt=self.data_entrada
        ).count()
        
        # Result is the sum plus one (for 1-based indexing)
        return higher_priority_count + same_priority_earlier_arrival + 1

class Barbeiro:
    STATUS_DISPONIVEL = 1
    STATUS_EM_ATENDIMENTO = 2
    STATUS_PAUSA = 3
    STATUS_INDISPONIVEL = 4
    
    def get_current_client(self):
        \"\"\"Get the current client being served by this barber\"\"\"
        if self.status != self.STATUS_EM_ATENDIMENTO:
            return None
        
        # Find the client entry that is marked with this barber
        try:
            return EntradaFila.objects.get(
                barbeiro_atendendo=self,
                status=EntradaFila.STATUS_EM_ATENDIMENTO
            )
        except EntradaFila.DoesNotExist:
            return None
'@; Set-Content -Path 'domain\models.py' -Value $content"

:: Now run the start_server.bat that has the UUID fixes
echo.
echo Iniciando servidor com todas as correcoes aplicadas...
call start_server.bat
