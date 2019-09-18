import ApiServiceCore from './ApiServiceCore';
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
        return ApiServiceCore.delete('/ApiEndpoint/DeleteMapping', { moduleId, apiEndpointId });
    }

    addMapping(moduleId, apiEndpointId) {
        return ApiServiceCore.post('/ApiEndpoint/AddMapping', { moduleId, apiEndpointId });
    }

    addSystemModuleGroup(data) {
        return ApiServiceCore.post('/ApiEndpoint/addSystemModuleGroup', data);
    }
    updateSystemModuleGroup(data) {
        return ApiServiceCore.put('/ApiEndpoint/UpdateSystemModuleGroup', data);
    }
    removeSystemModuleGroup(data) {
        return ApiServiceCore.delete('/ApiEndpoint/DeleteSystemModuleGroup', data);
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
        return ApiServiceCore.post('/ApiEndpoint/addModule', data);
    }
    updateModule(data) {
        return ApiServiceCore.put('/ApiEndpoint/updateModule', data);
    }
    removeModule(data) {
        return ApiServiceCore.delete('/ApiEndpoint/deleteModule', data);
    }
}
export default new ApiEndpointService();