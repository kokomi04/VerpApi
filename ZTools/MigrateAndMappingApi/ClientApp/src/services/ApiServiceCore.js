import axios, {AxiosInstance } from 'axios'
class ApiServiceCore {
    _axios;
    constructor() {
        const instance = axios.create({
            baseURL: '/api/',
            timeout: 30000,
            headers: { 'X-Custom-Header': 'my-dev' }
        });
        // Add a request interceptor
        instance.interceptors.request.use(function (config) {
            // Do something before request is sent
            return config;
        }, function (error) {
            // Do something with request error
            return Promise.reject(error);
        });

        // Add a response interceptor
        instance.interceptors.response.use(function (response) {
            // Do something with response data
            return response;
        }, function (error) {
            // Do something with response error
            return Promise.reject(error);
        });

        this._axios = instance;
    }

    get(url, params, configs = null) {
        return this._axios.get(url, { params: params, ...configs });
    }

    post(url, data, configs = null) {
        return this._axios.post(url, data, configs);
    }
    put(url, data, configs = null) {
        return this._axios.put(url, data, configs);
    }
    delete(url, data, configs = null) {
        return this._axios.delete(url, { data: data, ...configs });
    }
}
export default new ApiServiceCore();