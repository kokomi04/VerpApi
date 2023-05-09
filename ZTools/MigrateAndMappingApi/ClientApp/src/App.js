import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
//import Home from './components/Home';
import Counter from './components/Counter';
import FetchData from './components/FetchData';
import syncApiEndpoints from './components/mappings/sync-api-endpoints';
import systemModules from './components/system-modules/system-modules';
import { ModuleApiMapping } from './components/mappings/module-api-mapping';

import Login from './components/mappings/Login'
import './css/style.css'

export default () => (
    <Layout>
        <Route exact path='/' component={syncApiEndpoints} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetch-data/:startDateIndex?' component={FetchData} />
        <Route path='/sync-api-endpoint' component={syncApiEndpoints} />
        <Route path='/system-modules' component={systemModules} />
        <Route path='/module-apis-mapping/:moduleId?' component={ModuleApiMapping} />
        <Route path='/login' component={Login} />
        </Layout>
);
