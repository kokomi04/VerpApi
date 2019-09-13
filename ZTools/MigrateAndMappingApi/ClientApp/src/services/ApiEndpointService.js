import ApiServiceCore from './ApiServiceCore';
class ApiEndpointService {
    syncApiEndpoint() {
        return ApiServiceCore.get('/ApiEndpoint/SyncData');
    }
}
export default new ApiEndpointService();