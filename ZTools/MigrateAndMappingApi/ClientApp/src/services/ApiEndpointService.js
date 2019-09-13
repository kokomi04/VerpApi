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

    getSystemModules() {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModules');
    }
    getSystemModuleMapping(apiEndpointId) {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModulesMapping', { apiEndpointId });
    }
    getSystemModuleInfo(moduleId) {
        return ApiServiceCore.get('/ApiEndpoint/GetSystemModuleInfo', { moduleId });
    }
}
export default new ApiEndpointService();