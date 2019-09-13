import React, { Component } from 'react';
import ApiEndpointService from './../../services/ApiEndpointService'
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

export default class SystemModules extends Component {
    constructor(props) {
        super(props);
        this.state = {
            systemModules: []
        }
    }
    componentDidMount() {
        this.getSystemModules();
    }
    getSystemModules() {

        ApiEndpointService.getSystemModules()
            .then(r => {
                this.setState({
                    systemModules: r.data
                });
            });
    }

    render() {
        const { systemModules } = this.state;
        let rows = [];
        systemModules.forEach((row, index) => {
            rows.push(<tr key={index}>
                <th scope="row">{row.moduleId}</th>
                <td>
                    <NavLink tag={Link} className="btn btn-link" to={`/module-apis-mapping/${row.moduleId}`}>
                        {row.moduleName}
                    </NavLink>
                </td>
                <td>{row.description}</td>
            </tr>)
        });
        return (
            <div>
                <h4>Danh sách modules của hệ thống</h4>
                <table className="table">
                    <thead className="thead-light">
                        <tr>
                            <th scope="col">#</th>
                            <th scope="col">Name</th>
                            <th scope="col">Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </div>
        );
    }
}