# EuTôNaFila / QueueHub Documentation

## Overview

This directory contains the consolidated documentation for the EuTôNaFila queue management platform. The documentation has been streamlined to eliminate redundancy and provide a single source of truth for all project information.

## Documentation Structure

### Primary Documents

#### [INSTRUCTIONS.md](./INSTRUCTIONS.md)
**Complete Project Documentation** - The master document containing:
- Core development practices and guidelines
- Development environment setup
- Use cases and implementation progress
- API reference and testing strategies
- MySQL optimization and entity flattening
- Anonymous user system
- MCP Docker setup

#### [DEPLOYMENT.md](./DEPLOYMENT.md)
**Comprehensive Deployment Guide** - Complete deployment instructions:
- BoaHost Plesk deployment (recommended)
- Standalone server deployment
- Configuration files and environment variables
- Service management and monitoring
- Troubleshooting and maintenance
- Security considerations

#### [ARCHITECTURE.md](./ARCHITECTURE.md)
**Technical Architecture Documentation** - System design and patterns:
- Backend For Frontend (BFF) architecture
- URL structure and location slug system
- Database architecture and MySQL optimization
- Security model and role-based access
- Kiosk display architecture
- Performance considerations and future planning

### Legacy Documents

#### [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md)
**Legacy Deployment Guide** - Original deployment documentation (preserved for reference)

## Documentation Consolidation

### What Was Consolidated

The following documents have been consolidated into the primary documents above:

- ✅ **BOAHOST_DEPLOYMENT_GUIDE.md** → Merged into [DEPLOYMENT.md](./DEPLOYMENT.md)
- ✅ **BOAHOST_STANDALONE_DEPLOYMENT.md** → Merged into [DEPLOYMENT.md](./DEPLOYMENT.md)
- ✅ **data-type-mapping.md** → Merged into [ARCHITECTURE.md](./ARCHITECTURE.md) and [INSTRUCTIONS.md](./INSTRUCTIONS.md)
- ✅ **kiosk-queue-display-research.md** → Merged into [ARCHITECTURE.md](./ARCHITECTURE.md)
- ✅ **URL_STRUCTURE.md** → Merged into [ARCHITECTURE.md](./ARCHITECTURE.md)
- ✅ **SECURITY_MODEL.md** → Merged into [ARCHITECTURE.md](./ARCHITECTURE.md) and [INSTRUCTIONS.md](./INSTRUCTIONS.md)
- ✅ **KIOSK_DISPLAY_INTEGRATION.md** → Merged into [INSTRUCTIONS.md](./INSTRUCTIONS.md) and [ARCHITECTURE.md](./ARCHITECTURE.md)
- ✅ **MCP_DOCKER_SETUP.md** → Merged into [INSTRUCTIONS.md](./INSTRUCTIONS.md)
- ✅ **BFF_ARCHITECTURE.md** → Merged into [ARCHITECTURE.md](./ARCHITECTURE.md)

### Benefits of Consolidation

1. **Single Source of Truth**: All information is now in three primary documents
2. **Eliminated Redundancy**: Removed duplicate content across multiple files
3. **Improved Navigation**: Logical organization of related information
4. **Easier Maintenance**: Fewer files to update when making changes
5. **Better Searchability**: Related topics are grouped together

## Quick Reference

### For Developers
- Start with [INSTRUCTIONS.md](./INSTRUCTIONS.md) for development setup and guidelines
- Reference [ARCHITECTURE.md](./ARCHITECTURE.md) for system design patterns
- Use [DEPLOYMENT.md](./DEPLOYMENT.md) for production deployment

### For DevOps/Deployment
- Primary guide: [DEPLOYMENT.md](./DEPLOYMENT.md)
- Architecture context: [ARCHITECTURE.md](./ARCHITECTURE.md)

### For System Architects
- System design: [ARCHITECTURE.md](./ARCHITECTURE.md)
- Implementation details: [INSTRUCTIONS.md](./INSTRUCTIONS.md)

## Maintenance

When updating documentation:

1. **Update the appropriate primary document** (INSTRUCTIONS.md, DEPLOYMENT.md, or ARCHITECTURE.md)
2. **Update cross-references** if information moves between documents
3. **Maintain consistency** across all three primary documents
4. **Update this README** if the documentation structure changes

## Search Tips

To find specific information:

- **Development setup**: Search INSTRUCTIONS.md
- **Deployment procedures**: Search DEPLOYMENT.md
- **System architecture**: Search ARCHITECTURE.md
- **Use cases and APIs**: Search INSTRUCTIONS.md
- **Security and roles**: Search ARCHITECTURE.md
- **MySQL optimization**: Search ARCHITECTURE.md or INSTRUCTIONS.md

---

*Last updated: 2025-01-15*
