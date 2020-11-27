import React, { Component } from 'react';
import $ from 'jquery';
import { Typeahead } from 'react-bootstrap-typeahead';
import ApiEndpointService from '../../services/ApiEndpointService';
import Constants from './../../constants/Constants'

export default class ModalApiModuleMapping extends Component {
    constructor(props) {
        super(props);
        this.state = {
            isOpen: false,
            modules: null,
            selectedModule: null,
            apiEndpointId: null
        }
    }

    open(apiEndpointId) {
        this.setState({
            apiEndpointId: apiEndpointId
        })
        $(this.modal).modal();

        this.getSystemModules()();

        if (this.typeahead) {
            this.typeahead.getInstance().clear();
        }
    }

    getSystemModules = (num) => () => {
        ApiEndpointService.getSystemModules()
            .then((r) => {
                return r.data;
            })
            .then(modules => {
                ApiEndpointService.getSystemModuleMapping(this.state.apiEndpointId)
                    .then(mapping => {
                        var data = modules.filter(e => !mapping.data.find(m => m.moduleId === e.moduleId));
                        this.setState({
                            modules: data
                        });
                    });
            })
    }

    selectModule = (selecteds) => () => {
        if (selecteds && selecteds[0]) {
            console.log(selecteds[0]);
            this.setState({
                selectedModule: selecteds[0]
            })
        }
    }

    saveChange = () => () => {
        if (!this.state.selectedModule) {
            this.props.onSave(Promise.reject(`Bạn chưa chọn module nào`));
            return;
        }

        $(this.modal).modal('hide');

        this.props.onSave(ApiEndpointService.addMapping(this.state.selectedModule.moduleId, this.state.apiEndpointId));
    }

    render() {
        var options = [];
        if (this.state.modules) {
            options = this.state.modules;
            options.forEach(opt => {
                opt.strModuleId = `${opt.moduleId}`;
            })
        }

        return (
            <div ref={modal => this.modal = modal} className="modal fade" tabIndex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title" id="exampleModalLabel">Select module for mapping</h5>
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div className="modal-body">
                            {
                                (options && options.length > 0) ? (
                                    <Typeahead
                                        ref={(typeahead) => this.typeahead = typeahead}
                                        id='moduleId'
                                        filterBy={['moduleName', 'strModuleId']}
                                        labelKey={
                                            module =>
                                                `${module.moduleId} - ${module.moduleName}`
                                        }
                                        onChange={selected => this.selectModule(selected)()}
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