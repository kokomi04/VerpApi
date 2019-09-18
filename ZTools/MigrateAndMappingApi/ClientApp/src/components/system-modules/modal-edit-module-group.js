import React, { Component } from 'react';
import $ from 'jquery';
import { Typeahead } from 'react-bootstrap-typeahead';
import ApiEndpointService from '../../services/ApiEndpointService';
import Constants from './../../constants/Constants'

export default class ModalEditModuleGroup extends Component {
    isNew;
    constructor(props) {
        super(props);
        this.state = {
            info: {
                moduleGroupId: 0,
                moduleGroupName: '',
                sortOrder: 0
            }
        }
        this.open = this.open.bind(this);
        this.handlerChange = this.handlerChange.bind(this);
    }

    open(data) {
        this.state.info = {
            moduleGroupId: 0,
            moduleGroupName: '',
            sortOrder: 0
        };

        this.isNew = true;
        if (data && data.moduleGroupId) {
            this.isNew = false;

            this.state.info = {
                moduleGroupId: data.moduleGroupId,
                moduleGroupName: data.moduleGroupName,
                sortOrder: data.sortOrder
            };
        }

        $(this.modal).modal();
        
        this.setState({ info: this.state.info  });
    }

    handlerChange(event) {
        const target = event.target;
        const value = target.value;
        const name = target.name;
       
        this.state.info[name] = value;

        this.setState({
            info: this.state.info
        });
    }

    saveChange = () => () => {

        $(this.modal).modal('hide');
        if (!this.isNew) {
            this.props.onSave(ApiEndpointService.updateSystemModuleGroup(this.state.info));
        } else {
            this.props.onSave(ApiEndpointService.addSystemModuleGroup(this.state.info));
        }
    }

    render() {
        const { info } = this.state;

        return (
            <div ref={modal => this.modal = modal} className="modal fade" tabIndex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title" id="exampleModalLabel">
                                {!this.isNew ? `Edit module group` : `Add module group`}
                            </h5>
                            <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div className="modal-body">
                            <form>
                                <div className="form-group row">
                                    <label htmlFor="groupId" className="col-sm-2 col-form-label">ID</label>
                                    <div className="col-sm-10">
                                        <input type="text" className="form-control"
                                            name="moduleGroupId"
                                            readOnly={!this.isNew}
                                            value={this.state.info.moduleGroupId}
                                            onChange={this.handlerChange}
                                            ref="moduleGroupId"
                                        />
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="groupName" className="col-sm-2 col-form-label">Name</label>
                                    <div className="col-sm-10">
                                        <input type="text" className="form-control"
                                            name="moduleGroupName"
                                            value={this.state.info.moduleGroupName}
                                            onChange={this.handlerChange}
                                            ref="moduleGroupName"
                                        />
                                    </div>
                                </div>
                                <div className="form-group row">
                                    <label htmlFor="sortOrder" className="col-sm-2 col-form-label">Sort order</label>
                                    <div className="col-sm-10">
                                        <input type="text" className="form-control"
                                            name="sortOrder"
                                            value={this.state.info.sortOrder}
                                            onChange={this.handlerChange}
                                            ref="sortOrder"
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