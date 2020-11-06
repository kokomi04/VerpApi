import React, { Component } from 'react';
import $ from 'jquery';
import { Typeahead } from 'react-bootstrap-typeahead';
import ApiEndpointService from '../../services/ApiEndpointService';
import Constants from './../../constants/Constants'

export default class ModalEditModule extends Component {
    isNew;
    constructor(props) {
        super(props);
        this.state = {
            systemModuleGroups:[],
            info: {
                moduleId: 0,
                moduleGroupId: 0,
                moduleName: '',
                description: '',
                sortOrder: 0,
                isDeveloper: false
            }
        }
        this.open = this.open.bind(this);
        this.handlerChange = this.handlerChange.bind(this);
        this.saveChange = this.saveChange.bind(this);
    }

    open(data) {
        this.getSystemModuleGroups();
        this.state.info = {
            moduleId: 0,
            moduleGroupId: data ? data.moduleGroupId: 0,
            moduleName: '',
            description: '',
            sortOrder: 0,
            isDeveloper: false
        };

       
        $(this.modal).modal();
        this.isNew = true;

        if (data && data.moduleId > 0) {
            this.isNew = false;
            this.state.info = {
                moduleId: data.moduleId,
                moduleGroupId: data.moduleGroupId,
                moduleName: data.moduleName,
                description: data.description,
                sortOrder: data.sortOrder,
                isDeveloper: data.isDeveloper
            };
        }
        this.setState({
            info: this.state.info
        })
    }

    onLoadTag(event) {
        const target = event.target;
        if (target.type == 'checkbox') {
        }
    }

    handlerChange(event) {
        const target = event.target;
        let value = target.value;
        const name = target.name;

        if (target.type == 'checkbox') {
            value = target.checked;
        }
       
        this.state.info[name] = value;

        this.setState({
            info: this.state.info
        });
    }

    saveChange = () => () => {

        $(this.modal).modal('hide');
        if (!this.isNew) {
            this.props.onSave(ApiEndpointService.updateModule(this.state.info));
        } else {
            this.props.onSave(ApiEndpointService.addModule(this.state.info));
        }
    }

    getSystemModuleGroups() {

        return ApiEndpointService
            .getSystemModuleGroups()
            .then(r => {
                this.setState({
                    systemModuleGroups: r.data
                });
                return r;
            });
    }

    render() {
        let { systemModuleGroups } = this.state;
        let dropDownOptions = systemModuleGroups.map(g => (<option key={g.moduleGroupId} value={g.moduleGroupId}>{g.moduleGroupName}</option>));
        return (
            <div ref={modal => this.modal = modal} className="modal fade" tabIndex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title" id="exampleModalLabel">
                                {!this.isNew ? `Edit module` : `Add module`}
                            </h5>
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div className="modal-body">
                            <form>
                                <div className="form-group row">
                                    <label htmlFor="moduleId" className="col-sm-4 col-form-label">Group</label>
                                    <div className="col-sm-8">
                                        <select
                                            className="form-control"
                                            name="moduleGroupId"
                                            value={this.state.info.moduleGroupId}
                                            onChange={this.handlerChange}
                                        >
                                            <option>--Select--</option>
                                            {dropDownOptions}
                                        </select>
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="moduleId" className="col-sm-4 col-form-label">ID</label>
                                    <div className="col-sm-8">
                                        <input type="text" className="form-control"
                                            name="moduleId"
                                            readOnly={!this.isNew}
                                            checked={this.state.info.moduleId}
                                            onChange={this.handlerChange}
                                        />
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="moduleName" className="col-sm-4 col-form-label">Name</label>
                                    <div className="col-sm-8">
                                        <input type="text" className="form-control"
                                            name="moduleName"
                                            value={this.state.info.moduleName}
                                            onChange={this.handlerChange}                                            
                                        />
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="description" className="col-sm-4 col-form-label">Description</label>
                                    <div className="col-sm-8">
                                        <textarea type="text" className="form-control"
                                            name="description"                                            
                                            onChange={this.handlerChange}
                                            value={this.state.info.description}
                                        ></textarea>
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="sortOrder" className="col-sm-4 col-form-label">Sort order</label>
                                    <div className="col-sm-8">
                                        <input type="text" className="form-control"
                                            name="sortOrder"
                                            value={this.state.info.sortOrder}
                                            onChange={this.handlerChange}
                                        />
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="isDeveloper" className="col-sm-4 col-form-label">Nhà phát triển</label>
                                    <div className="col-sm-8">
                                        <input type="checkbox" className="form-control checkbox"
                                            name="isDeveloper"
                                            checked={this.state.info.isDeveloper}
                                            onChange={this.handlerChange}
                                        />
                                    </div>
                                </div>
                            </form>

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