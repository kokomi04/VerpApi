import React, { Component } from 'react';
import $ from 'jquery';
import { Typeahead } from 'react-bootstrap-typeahead';
import ApiEndpointService from '../../services/ApiEndpointService';
import Constants from './../../constants/Constants'

export default class ModalModuleApiMapping extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isOpen: false,
            endpoints: null,
            selectedEndpoint: null
        }
    }

    open() {
        $(this.modal).modal();
        this.getApiEndpoint()();
        if (this.typeahead) {
            this.typeahead.getInstance().clear();
        }
    }

    getApiEndpoint = (num) => () => {
        ApiEndpointService.getApiEndpoint()
            .then((r) => {
                return r.data;
            })
            .then(endpoints => {
                ApiEndpointService.getApiEndpointMapping(this.props.moduleId)
                    .then(mapping => {
                        var data = endpoints.filter(e => !mapping.data.find(m => m.apiEndpointId === e.apiEndpointId));
                        this.setState({
                            endpoints: data
                        });
                    });
            })
    }

    selectEndpoint = (selecteds) => () => {
        if (selecteds && selecteds[0]) {
            console.log(selecteds[0]);
            this.setState({
                selectedEndpoint: selecteds[0]
            })
        }
    }

    saveChange = () => () => {
        if (!this.state.selectedEndpoint) {
            this.props.onSave(Promise.reject(`Bạn chưa chọn endpoint nào`));
            return;
        }

        $(this.modal).modal('hide');

        this.props.onSave(ApiEndpointService.addMapping(this.props.moduleId, this.state.selectedEndpoint.id));
    }

    render() {
        var options = [];
        if (this.state.endpoints) {
            this.state.endpoints.forEach((endpoint) => {
                options.push({
                    id: endpoint.apiEndpointId,
                    serviceId: endpoint.serviceId,
                    route: endpoint.route,
                    methodId: endpoint.methodId,
                    methodName: Constants.Methods.find(m => m.id === endpoint.methodId).name,
                    actionId: endpoint.actionId,
                    actionName: Constants.Actions.find(m => m.id === endpoint.actionId).name,
                    search: `${this.methodName} ${endpoint.route} ${this.actionName}`
                })
            });
            
        }

        return (
            <div ref={modal => this.modal = modal} className="modal fade" tabIndex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
                <div className="modal-dialog modal-xl" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title" id="exampleModalLabel">Select api endpoint for mapping</h5>
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div className="modal-body">
                            {
                                (options && options.length > 0) ? (
                                    <Typeahead
                                        ref={(typeahead) => this.typeahead = typeahead}
                                        id='id'
                                        filterBy={['search']}
                                        labelKey={
                                            endpoint =>
                                                `${Constants.Services.find(s => s.serviceId == endpoint.serviceId).serviceName} --- ${endpoint.methodName} --- ${endpoint.route} ${endpoint.actionName}`
                                        }
                                        onChange={selected => this.selectEndpoint(selected)()}
                                        options={options}
                                    />
                                ) : (<div />)
                            }

                        </div>
                        <div className="modal-footer">
                            <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>
                            <button type="button" className="btn btn-primary" onClick={this.saveChange()}>Save changes</button>
                        </div>
                    </div>
                </div>
            </div>
        )
    }
}