import axios, { AxiosInstance } from 'axios'
//import * as axios from 'axios'
class ApiServiceCore {
    _axios = null;
    _configs = null;
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
        if (localStorage.getItem('expires_at') < new Date().getTime()) {
            localStorage.removeItem('access_token');
            localStorage.removeItem('expires_at')
        }
        this._configs = { headers: { 'Authorization': `Bearer ${localStorage.getItem('access_token')}` } };
    }

    get(url, params, configs = this._configs) {
        if (url.indexOf('http') == 0) {
            return axios.create().get(url, { params: params, ...configs });
        }
        return this._axios.get(url, { params: params, ...configs });
    }

    post(url, data, configs = this._configs) {
        return this._axios.post(url, data, configs);
    }
    put(url, data, configs = this._configs) {
        return this._axios.put(url, data, configs);
    }
    delete(url, data, configs = this._configs) {
        return this._axios.delete(url, { data: data, ...configs });
    }
}
const instance = new ApiServiceCore();
export default instance;