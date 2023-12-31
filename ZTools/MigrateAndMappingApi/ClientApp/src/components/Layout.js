import React from 'react';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';
import Login from './mappings/Login';
import ApiEndpointService from '../services/ApiEndpointService';

export default props => (
    <div>
        {localStorage.getItem('access_token') == null ? < Login /> : (
            < div>
                < NavMenu />
                <Container fluid={true}>
                    {props.children}
                </Container>
            </div>
        )}
    </div>
);
