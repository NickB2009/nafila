# 🚀 Production Deployment Guide
# Phase 9: Complete deployment and go-live guide

## 📋 Overview

This guide covers the complete deployment process for the GrandeTech QueueHub application with MySQL migration from SQL Server.

## 🎯 Prerequisites

### System Requirements
- **OS**: Windows Server 2019+ or Linux (Ubuntu 20.04+)
- **RAM**: Minimum 8GB, Recommended 16GB+
- **CPU**: Minimum 4 cores, Recommended 8+ cores
- **Storage**: Minimum 100GB SSD, Recommended 500GB+ SSD
- **Network**: Stable internet connection with static IP

### Software Requirements
- **Docker**: 20.10+
- **Docker Compose**: 2.0+
- **PowerShell**: 7.0+ (Windows) or PowerShell Core (Linux)
- **Python**: 3.8+ (for data transformation)
- **MySQL Client**: 8.0+
- **SQL Server Client**: For data export

## 🔧 Pre-Deployment Setup

### 1. Environment Configuration

#### Staging Environment
```bash
# Copy staging environment template
cp production.env.template staging.env

# Edit staging.env with your staging values
nano staging.env
```

#### Production Environment
```bash
# Copy production environment template
cp production.env.template production.env

# Edit production.env with your production values
nano production.env
```

### 2. SSL Certificate Setup
```bash
# Create SSL directory
mkdir -p nginx/ssl

# Generate self-signed certificate for testing
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/key.pem \
  -out nginx/ssl/cert.pem

# For production, use a proper SSL certificate from a CA
```

### 3. Database Backup
```bash
# Create backup directory
mkdir -p backups

# Backup current SQL Server data
sqlcmd -S "your-sql-server" -E -Q "BACKUP DATABASE QueueHubDb TO DISK = 'backups/QueueHubDb_pre_migration.bak'"
```

## 🚀 Deployment Process

### Phase 1: Staging Deployment

#### 1.1 Deploy to Staging
```bash
# Start staging environment
docker-compose -f docker-compose.staging.yml --env-file staging.env up -d

# Check service status
docker-compose -f docker-compose.staging.yml ps

# View logs
docker-compose -f docker-compose.staging.yml logs -f
```

#### 1.2 Test Staging Environment
```bash
# Test API health
curl http://localhost:8080/health

# Test database connectivity
curl http://localhost:8080/api/health/database

# Run migration tests
powershell -ExecutionPolicy Bypass -File "scripts/run-mysql-tests.ps1"
```

#### 1.3 Validate Staging
```bash
# Run comprehensive validation
powershell -ExecutionPolicy Bypass -File "scripts/simple-validation.ps1"

# Test all API endpoints
powershell -ExecutionPolicy Bypass -File "scripts/test-api-endpoints.ps1"
```

### Phase 2: Production Migration

#### 2.1 Pre-Migration Checklist
- [ ] Staging environment validated
- [ ] All tests passing
- [ ] Performance benchmarks acceptable
- [ ] Backup created
- [ ] Rollback plan ready
- [ ] Team notified of maintenance window

#### 2.2 Execute Production Migration
```bash
# Run production migration
powershell -ExecutionPolicy Bypass -File "scripts/execute-production-migration.ps1"
```

#### 2.3 Post-Migration Validation
```bash
# Validate migration success
powershell -ExecutionPolicy Bypass -File "scripts/validate-complete-migration.ps1"

# Test all functionality
powershell -ExecutionPolicy Bypass -File "scripts/test-production-functionality.ps1"
```

## 📊 Monitoring and Alerting

### 1. Access Monitoring Dashboards
- **Grafana**: http://your-server:3000
- **Prometheus**: http://your-server:9090
- **API Health**: http://your-server:8080/health

### 2. Key Metrics to Monitor
- **API Response Time**: < 200ms average
- **Database Connections**: < 80% of max
- **Memory Usage**: < 85%
- **CPU Usage**: < 80%
- **Queue Length**: < 100 customers
- **Error Rate**: < 1%

### 3. Alert Configuration
Alerts are configured in `monitoring/alert_rules.yml` and will notify via:
- Email notifications
- Slack webhooks
- PagerDuty integration

## 🔄 Rollback Procedures

### Emergency Rollback
```bash
# Execute rollback script
powershell -ExecutionPolicy Bypass -File "scripts/rollback-migration.ps1"
```

### Manual Rollback Steps
1. Stop all services
2. Restore SQL Server from backup
3. Restart with SQL Server configuration
4. Validate functionality
5. Investigate migration issues

## 🛠️ Maintenance Tasks

### Daily Tasks
- [ ] Check service health
- [ ] Review error logs
- [ ] Monitor performance metrics
- [ ] Verify backup completion

### Weekly Tasks
- [ ] Review security logs
- [ ] Update dependencies
- [ ] Performance optimization review
- [ ] Capacity planning

### Monthly Tasks
- [ ] Security audit
- [ ] Performance analysis
- [ ] Disaster recovery testing
- [ ] Documentation updates

## 🔒 Security Considerations

### 1. Environment Variables
- Store sensitive data in environment variables
- Use strong, unique passwords
- Rotate secrets regularly
- Never commit secrets to version control

### 2. Network Security
- Use HTTPS in production
- Configure firewall rules
- Enable DDoS protection
- Implement rate limiting

### 3. Database Security
- Use SSL/TLS for database connections
- Implement least privilege access
- Enable audit logging
- Regular security updates

## 📈 Performance Optimization

### 1. Database Optimization
```sql
-- Run optimization scripts
mysql -u root -p < scripts/mysql-optimization-indexes.sql
mysql -u root -p < scripts/mysql-performance-config.sql
```

### 2. Application Optimization
- Enable response compression
- Configure caching
- Optimize connection pooling
- Monitor memory usage

### 3. Infrastructure Optimization
- Use SSD storage
- Optimize Docker resource limits
- Configure load balancing
- Implement CDN if needed

## 🚨 Troubleshooting

### Common Issues

#### 1. Service Won't Start
```bash
# Check logs
docker-compose logs service-name

# Check resource usage
docker stats

# Restart service
docker-compose restart service-name
```

#### 2. Database Connection Issues
```bash
# Test database connectivity
mysql -h localhost -u root -p -e "SELECT 1"

# Check database logs
docker-compose logs mysql
```

#### 3. High Memory Usage
```bash
# Check memory usage
docker stats

# Restart services
docker-compose restart

# Check for memory leaks
docker-compose logs api | grep -i memory
```

### Emergency Contacts
- **System Administrator**: [Contact Info]
- **Database Administrator**: [Contact Info]
- **Development Team**: [Contact Info]
- **On-Call Engineer**: [Contact Info]

## 📚 Additional Resources

### Documentation
- [MySQL Migration Guide](MYSQL_MIGRATION_GUIDE.md)
- [API Documentation](API_DOCUMENTATION.md)
- [Troubleshooting Guide](TROUBLESHOOTING.md)

### Monitoring
- [Grafana Dashboards](monitoring/grafana/)
- [Prometheus Configuration](monitoring/prometheus.yml)
- [Alert Rules](monitoring/alert_rules.yml)

### Scripts
- [Migration Scripts](scripts/)
- [Validation Scripts](scripts/)
- [Monitoring Scripts](scripts/)

## ✅ Deployment Checklist

### Pre-Deployment
- [ ] Environment configured
- [ ] SSL certificates ready
- [ ] Database backup created
- [ ] Team notified
- [ ] Rollback plan ready

### During Deployment
- [ ] Staging deployment successful
- [ ] All tests passing
- [ ] Performance acceptable
- [ ] Migration executed
- [ ] Services started
- [ ] Health checks passing

### Post-Deployment
- [ ] All functionality working
- [ ] Monitoring active
- [ ] Alerts configured
- [ ] Documentation updated
- [ ] Team notified of success

## 🎉 Go-Live

Once all validation is complete and the system is running smoothly:

1. **Update DNS/Load Balancer** to point to new MySQL backend
2. **Monitor closely** for the first 24 hours
3. **Have rollback plan ready** for first 48 hours
4. **Schedule decommission** of old SQL Server after 1 week
5. **Celebrate** the successful migration! 🎊

---

**Remember**: This is a production system handling real customer data. Always prioritize data integrity and system stability over speed. When in doubt, rollback and investigate.
