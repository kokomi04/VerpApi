﻿import ApiServiceCore from './ApiServiceCore';
class ApiEndpointService {
    getApiEndpoint() {
        return ApiServiceCore.get('/ApiEndpoint/GetApiEndpoints');
    }
    getApiEndpointMapping(moduleId = -1) {
        return ApiServiceCore.get('/ApiEndpoint/GetApiEndpointsMapping', { moduleId });
    }
    syncApiEndpoint() {
        return ApiServiceCore.post('/ApiEndpoint/SyncApiEndpoints');
    }

    removeMapping(moduleId, apiEndpointId) {
        return ApiServiceCore.delete('/ApiEndpoint/DeleteMapping', { moduleId, apiEndpointId }).then(() => this.cleanCache());
    }

    addMapping(moduleId, apiEndpointId) {
        return ApiServiceCore.post('/ApiEndpoint/AddMapping', { moduleId, apiEndpointId }).then(() => this.cleanCache());
    }

    addSystemModuleGroup(data) {
        return ApiServiceCore.post('/ApiEndpoint/addSystemModuleGroup', data).then(() => this.cleanCache());
    }
    updateSystemModuleGroup(data) {
        return ApiServiceCore.put('/ApiEndpoint/UpdateSystemModuleGroup', data).then(() => this.cleanCache());
    }
    removeSystemModuleGroup(data) {
        return ApiServiceCore.delete('/ApiEndpoint/DeleteSystemModuleGroup', data).then(() => this.cleanCache());
    }

    getSystemModuleGroups() {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModuleGroups');
    }

    getSystemModules() {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModules');
    }
    getSystemModuleMapping(apiEndpointId) {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModulesMapping', { apiEndpointId });
    }
    getSystemModuleInfo(moduleId) {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModuleInfo', { moduleId });
    }

    addModule(data) {
        return ApiServiceCore.post('/ApiEndpoint/addModule', data).then(() => this.cleanCache());
    }
    updateModule(data) {
        return ApiServiceCore.put('/ApiEndpoint/updateModule', data).then(() => this.cleanCache());
    }
    removeModule(data) {
        return ApiServiceCore.delete('/ApiEndpoint/deleteModule', data).then(() => this.cleanCache());
    }
    Login(username, password) {
        return ApiServiceCore;
    }
    async getConfig() {
        return await ApiServiceCore.get('/login/GetConfigs');
    }
    cleanCache() {
        return ApiServiceCore.get('https://test-app.verp.vn/endpoint/api/roles/AuthCacheRemove')
    }

}
export default new ApiEndpointService();