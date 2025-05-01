"""
Helper script to run the Celery worker for development.
"""
import os
import subprocess
import sys
import time

def run_celery_worker():
    """Run the Celery worker process."""
    # Set the Django settings module
    os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'eutonafila.settings')
    
    # Build the celery command
    command = [
        sys.executable,  # Python executable
        '-m', 'celery',
        '-A', 'eutonafila',
        'worker',
        '--loglevel=info',
        # '--concurrency=1',  # Uncomment for single worker mode
    ]
    
    # Run the command
    process = subprocess.Popen(command)
    
    try:
        print("Celery worker started. Press CTRL+C to stop.")
        # Keep the script running
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("Stopping Celery worker...")
        process.terminate()
        process.wait()
        print("Celery worker stopped.")

if __name__ == '__main__':
    run_celery_worker() 