    def get_position(self):
        """Get position in queue (FIFO order)"""
        if self.status != self.STATUS_AGUARDANDO:
            return 0
        
        # Simply use position number (FIFO order)
        return self.position_number 

    def get_priority_position(self):
        """Get position in queue, considering priority level"""
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
            created_at__lt=self.created_at
        ).count()
        
        # Final position is the sum of both counts, plus 1 (1-indexed)
        return higher_priority_count + same_priority_earlier_arrival + 1 

    def _notify_status_change(self):
        """Notify WebSocket clients about status change"""
        try:
            from application.utils.websocket_utils import notifier
            
            # Notify the individual queue entry
            notifier.notify_queue_entry(
                str(self.id),
                self.status,
                self.get_position(),
                self.format_wait_time()
            )
            
            # Notify the barbershop
            notifier.notify_queue_update(self.barbearia.slug)
            
        except Exception as e:
            import logging
            logger = logging.getLogger(__name__)
            logger.error(f"Error notifying status change: {e}", exc_info=True)
            # Fail gracefully if notification fails 