import React, { Component } from 'react';
import ApiEndpointService from '../../services/ApiEndpointService';
import Constants from './../../constants/Constants'
import ModalModuleApiMapping from './modal-module-api-mapping';
import { ToastContainer } from 'react-toastr'

export class ModuleApiMapping extends Component {
    container;
    constructor(props) {
        super(props);
        this.state = {
            moduleId: 0,
            moduleInfo: null,
            endpoints: [],
            isOpenForAdd: false
        }
        this.onSave = this.onSave.bind(this);
    }
    componentDidMount() {
        let moduleId = this.props.match.params.moduleId;

        this.setState({
            moduleId: moduleId
        });

        this.getModuleInfo(moduleId);
        this.getApiEndpointsMapping(moduleId);
    }

    getModuleInfo(moduleId) {
        ApiEndpointService.getSystemModuleInfo(moduleId)
            .then(r => {
                this.setState({
                    moduleInfo: r.data
                });
            });

    }
    getApiEndpointsMapping(moduleId) {
        ApiEndpointService.getApiEndpointMapping(moduleId)
            .then((r) => {
                this.setState({
                    endpoints: r.data
                });
            });
    }
    openForAdd = () => () => {

        console.log('openForAdd');

        this.setState({
            isOpenForAdd: true
        })

        this.refs.modalForAdd.open();
    }
    onSave(saveAction) {
        saveAction
            .then(r => {
                this.container.success('Thành công');
                this.getApiEndpointsMapping(this.state.moduleId);
            })
            .catch(e => {
                this.container.error(
                    e.message ? e.message : e
                    , '', {
                        closeButton: true,
                    });
            });
    }
    remove = (apiEndpointId) => () => {
        let moduleId = this.props.match.params.moduleId;
        ApiEndpointService
            .removeMapping(moduleId, apiEndpointId)
            .then(() => {
                this.container.warning('Xóa thành công');
                this.getApiEndpointsMapping(this.state.moduleId);
            })
            .catch(e => {
                this.container.error(
                    e.message ? e.message : e
                    , '', {
                        closeButton: true,
                    });
            })
    }

    render() {
        var rows = [];
        const { endpoints } = this.state;
        endpoints.forEach((row, index) => {
            rows.push(<tr key={index}>
                <th scope="row">{row.apiEndpointId}</th>
                <td>{Constants.Services.find(m => m.serviceId === row.serviceId).serviceName}</td>
                <td>{Constants.Methods.find(m => m.id === row.methodId).name}</td>
                <td>{row.route}</td>
                <td>{Constants.Actions.find(m => m.id === row.actionId).name}</td>
                <td>
                    <button className="btn btn-xs btn-danger" onClick={this.remove(row.apiEndpointId)}>Remove</button>
                </td>
            </tr>);
        });
        const style = {
            margin: '10px'
        };
        return (
            <div>
                {
                    this.state.moduleInfo ?
                        (
                            <div>
                                <ToastContainer ref={ref => this.container = ref}
                                    className="toast-top-right" />

                                <div>
                                    <h3>Module - Api endpoints mapping <span className="badge badge-secondary">{this.state.moduleInfo.moduleName}</span></h3>
                                    <div className="alert alert-primary" role="alert">
                                        {this.state.moduleInfo.description}
                                    </div>
                                </div>

                                <div className="card">
                                    <div className="card-header">
                                        {this.state.moduleInfo.moduleName}
                                    </div>
                                    <div className="card-body">
                                        <h5 className="card-title">{this.state.moduleInfo.description}</h5>

                                        <table className="table">
                                            <thead className="thead-light">
                                                <tr>
                                                    <th scope="col">#</th>
                                                    <th scope="col">Service</th>
                                                    <th scope="col">Method</th>
                                                    <th scope="col">Route</th>
                                                    <th scope="col">Action</th>
                                                    <th scope="col"></th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {rows}
                                            </tbody>
                                        </table>

                                        {
                                            rows.length === 0 ?
                                                (
                                                    <div className="alert alert-warning" role="alert">
                                                        No records found!
                                                    </div>
                                                ) :
                                                (
                                                    null
                                                )
                                        }

                                        <a href="/" className="btn btn-link" style={style}>Api endpoints</a>
                                        <a className="btn btn-success" onClick={this.openForAdd()} style={style}>Add</a>
                                    </div>
                                </div>
                                <ModalModuleApiMapping ref="modalForAdd" moduleId={this.state.moduleId} onSave={this.onSave} />
                            </div>
                        ) :
                        (<div></div>)
                }
            </div>
        )
    }
}