EuTÙNaFila - KingHost Deployment Guide
================================

Pre-deployment Requirements:
-------------------------
1. KingHost account with Python support
2. Domain configured (eutonafila.com.br)
3. MySQL database credentials
4. SSL certificate (recommended)

Deployment Steps:
--------------
1. File Upload:
   - Upload all contents of this package to your KingHost directory
   - Set permissions:
     * chmod 755 for directories
     * chmod 644 for files
     * chmod 755 for deploy.sh

2. Environment Setup:
   - Copy .env.example to .env
   - Update .env with your credentials
   - Ensure Python 3.8+ is available

3. Database Configuration:
   - Create MySQL database via KingHost panel
   - Update database credentials in .env

4. Deployment:
   - Connect via SSH to your KingHost server
   - Navigate to project directory
   - Run: ./deploy.sh

5. Web Server Setup:
   - Configure Python WSGI in KingHost panel
   - Point to passenger_wsgi.py
   - Set up static files directory

6. SSL Configuration:
   - Enable SSL in KingHost panel
   - Verify HTTPS redirects

7. Verification:
   - Visit https://eutonafila.com.br
   - Check admin interface
   - Monitor logs for errors

Troubleshooting:
--------------
1. Static Files Issues:
   - Verify STATIC_ROOT in settings
   - Check directory permissions
   - Run collectstatic manually

2. Database Connection:
   - Verify credentials in .env
   - Test database connection
   - Check KingHost MySQL status

3. 500 Server Errors:
   - Check error logs
   - Verify all requirements installed
   - Check file permissions

Support:
-------
For hosting related issues:
- KingHost Support: https://king.host/suporte
- Django Deployment Docs: https://docs.djangoproject.com/en/5.0/howto/deployment/