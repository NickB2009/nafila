// Cursor Rules and Status Tracking for EuTôNaFila
// This file helps track the status of different components and features

const CursorStatus = {
    DONE: '🟢 [DONE]',
    WIP: '🟡 [WIP]',
    TODO: '🔴 [TODO]',
    BLOCKED: '⚠️ [BLOCKED]'
};

const ProjectStatus = {
    // Core Features
    core: {
        multiTenant: CursorStatus.DONE,
        queueManagement: CursorStatus.DONE,
        realTimeUpdates: CursorStatus.WIP,
        analyticsDashboard: CursorStatus.TODO
    },

    // Infrastructure
    infrastructure: {
        mysqlSetup: CursorStatus.DONE,
        inMemoryCache: CursorStatus.DONE,
        performanceOptimization: CursorStatus.WIP,
        automatedBackups: CursorStatus.TODO
    },

    // UI/UX Components
    ui: {
        darkTheme: CursorStatus.DONE,
        responsiveDesign: CursorStatus.DONE,
        mobileApp: CursorStatus.WIP,
        offlineMode: CursorStatus.TODO
    },

    // Screens Status (based on mockups)
    screens: {
        login: CursorStatus.DONE,
        telaInicial: CursorStatus.DONE,
        contaUsuario: CursorStatus.DONE,
        checkIn: CursorStatus.DONE,
        loginBarbeiro: CursorStatus.DONE,
        painelAdmin: CursorStatus.DONE,
        painelBarbeiro: CursorStatus.DONE,
        painelFila: CursorStatus.DONE,
        telaAutoatendimento: CursorStatus.DONE
    }
};

// Function to update status
function updateStatus(category, feature, status) {
    if (ProjectStatus[category] && ProjectStatus[category][feature]) {
        ProjectStatus[category][feature] = status;
        console.log(`Updated ${category}.${feature} to ${status}`);
    }
}

// Function to get current status
function getStatus(category, feature) {
    return ProjectStatus[category]?.[feature] || CursorStatus.TODO;
}

// Function to generate status report
function generateStatusReport() {
    let report = '📋 EuTôNaFila Project Status Report\n\n';
    
    for (const [category, features] of Object.entries(ProjectStatus)) {
        report += `## ${category.charAt(0).toUpperCase() + category.slice(1)}\n`;
        for (const [feature, status] of Object.entries(features)) {
            report += `- ${status} ${feature}\n`;
        }
        report += '\n';
    }
    
    return report;
}

// Export for use in other files
module.exports = {
    CursorStatus,
    ProjectStatus,
    updateStatus,
    getStatus,
    generateStatusReport
}; 