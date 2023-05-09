import React from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import ApiEndpointService from '../services/ApiEndpointService';
import { ToastContainer } from 'react-toastr';

export default class NavMenu extends React.Component {
    container;
    constructor(props) {
        super(props);

        this.toggle = this.toggle.bind(this);
        this.state = {
            isOpen: false
        };
    }
    toggle() {
        this.setState({
            isOpen: !this.state.isOpen
        });
    }

    cleanCache() {
        ApiEndpointService.cleanCache().then(r => {
            this.container.success('Thành công');
        }).catch(e => {
            this.container.error(
                e.message ? e.message : e
                , '', {
                closeButton: true,
            });
        })
    }

    render() {
        return (
            <header>
                <ToastContainer ref={ref => this.container = ref}
                    className="toast-top-right" />
                <Navbar className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light >
                    <Container>
                        <NavbarBrand tag={Link} to="/">Migrate api endpoints AND mapping modules - apis</NavbarBrand>
                        <NavbarToggler onClick={this.toggle} className="mr-2" />
                        <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={this.state.isOpen} navbar>
                            <ul className="navbar-nav flex-grow">
                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                                </NavItem>

                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/sync-api-endpoint">Endpoints</NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/system-modules">Modules</NavLink>
                                </NavItem>
                                <NavItem>
                                    <button onClick={() => this.cleanCache()}>Clean cache</button>
                                </NavItem>
                            </ul>
                        </Collapse>
                    </Container>
                </Navbar>
            </header>
        );
    }
}
