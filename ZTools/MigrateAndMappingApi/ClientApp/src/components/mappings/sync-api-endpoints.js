import React, { Component } from 'react';
import { connect } from 'react-redux';
import ApiEndpointService from './../../services/ApiEndpointService';
import { ToastContainer } from 'react-toastr'
//import { ToastMessage } from "react-toastr";
import Constants from './../../constants/Constants'
import ModalApiModuleMapping from './modal-api-module-mapping';


class SyncApiEndpoint extends Component {
    container;
    actions = {
        GET_API_ENDPOINTS: 1,
        SYNC_API_ENDPOINTS: 2
    };

    constructor(props) {
        super(props);
        this.state = {
            loading: false,
            endpoints: [],
            currentAction: 0
        };

        this.onSave = this.onSave.bind(this);
    }


    componentDidMount() {
        this.getApiEndpoint(0)();
    }

    syncApiEndpoint = (num) => () => {
        console.log(num);
        this.setState({
            loading: true,
            currentAction: this.actions.SYNC_API_ENDPOINTS
        })
        ApiEndpointService.syncApiEndpoint()
            .then((r) => {
                this.setState({
                    loading: false,
                    currentAction: 0
                });
                this.container.success(
                    `Thành công`
                    , '', {
                        closeButton: true,
                    });
            })
            .catch((e) => {
                this.setState({
                    loading: false,
                    currentAction: 0
                });

                this.container.error(
                    e.message
                    , '', {
                        closeButton: true,
                    });
            });
    }

    getApiEndpoint = (num) => () => {
        console.log(num);
        this.setState({
            loading: true,
            currentAction: this.actions.GET_API_ENDPOINTS
        })
        ApiEndpointService.getApiEndpoint()
            .then((r) => {
                this.setState({
                    loading: false,
                    endpoints: r.data,
                    currentAction: 0
                });
            })
            .catch((e) => {
                this.setState({
                    loading: false,
                    currentAction: 0
                });

                this.container.error(
                    e.message
                    , '', {
                        closeButton: true,
                    });
            });
    }
    openForMapping = (apiEndpointId) => () => {
        this.refs.modalForAddModule.open(apiEndpointId);
    }
    onSave(saveAction) {

        saveAction
            .then(r => {
                this.container.success('Thành công');
                this.getApiEndpoint();
            })
            .catch(e => {
                this.container.error(
                    e.message ? e.message : e
                    , '', {
                        closeButton: true,
                    });
            });
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
                    <button className="btn btn-success" onClick={this.openForMapping(row.apiEndpointId)}>Map to module</button>
                </td>
            </tr>);
        });

        const style = {
            margin: '10px'
        };

        return (
            <div>
                <ToastContainer ref={ref => this.container = ref}
                    className="toast-top-right" />

                <button className="btn btn-primary" style={style} type="button" disabled={this.state.loading} onClick={this.syncApiEndpoint(1)} >
                    <span className={"spinner-border spinner-border-sm"} hidden={this.state.currentAction !== this.actions.SYNC_API_ENDPOINTS} role="status" aria-hidden="true"></span>
                    Sync api endpoints
                </button>

                <button className="btn btn-info" style={style} type="button" disabled={this.state.loading} onClick={this.getApiEndpoint(1)} >
                    <span className={"spinner-border spinner-border-sm"} hidden={this.state.currentAction !== this.actions.GET_API_ENDPOINTS} role="status" aria-hidden="true"></span>
                    Get api endpoints
                </button>

                <div>
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
                </div>

                <ModalApiModuleMapping ref="modalForAddModule" onSave={this.onSave} />
            </div>
        );
    }
}

export default connect()(SyncApiEndpoint);
