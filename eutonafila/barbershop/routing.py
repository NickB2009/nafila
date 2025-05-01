from django.urls import re_path
from . import consumers

websocket_urlpatterns = [
    re_path(r'ws/queue/(?P<queue_id>[0-9a-f-]+)/$', consumers.QueueUpdatesConsumer.as_asgi()),
    re_path(r'ws/barbershop/(?P<slug>[\w-]+)/$', consumers.BarbershopQueueConsumer.as_asgi()),
] 