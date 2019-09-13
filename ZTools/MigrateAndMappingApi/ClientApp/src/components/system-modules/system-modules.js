import React, { Component } from 'react';
import ApiEndpointService from './../../services/ApiEndpointService'
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import { ToastContainer } from 'react-toastr';
import ModalEditModuleGroup from './modal-edit-module-group';
import ModalEditModule from './modal-edit-module';
import $ from 'jquery';

export default class SystemModules extends Component {
    constructor(props) {
        super(props);
        this.state = {
            systemModuleGroups: [],
            systemModules: []
        }
        this.saveModuleGroup = this.saveModuleGroup.bind(this);
        this.saveModule = this.saveModule.bind(this);
    }
    componentDidMount() {
        this.getSystemModules();
    }

    getSystemModules() {
        this.getSystemModuleGroups()
            .then(() => {
                return ApiEndpointService.getSystemModules();
            })
            .then(r => {
                this.setState({
                    systemModules: r.data
                });
            });

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

    openEditGroup = (data) => () => {
        this.refs.modalEditModuleGroup.open(data);
    }

    delGroup = (data) => () => {
        if (window.confirm(`Bạn muốn xóa nhóm ${data.moduleGroupName} với tất cả các phân quyền liên quan không?`)) {
            ApiEndpointService.removeSystemModuleGroup(data)
                .then(r => {
                    this.container.warning('Xóa thành công!');
                    this.getSystemModules();
                })
                .catch(e => {
                    this.container.error(
                        e.message ? e.message : e
                        , '', {
                            closeButton: true,
                        });
                });
        }
    }
    saveModuleGroup(saveAction) {
        saveAction
            .then(r => {
                this.container.success('Thành công');
                this.getSystemModules();
            })
            .catch(e => {
                this.container.error(
                    e.message ? e.message : e
                    , '', {
                        closeButton: true,
                    });
            });
    }

    openEditModule = (data) => () => {
        if (!data || !data.moduleId) {
            data = {
                moduleGroupId: $('.nav-link.active', this.refs.tabGroups).data('module-groupid')
            }
        }
        this.refs.modalEditModule.open(data);
    }

    saveModule(saveAction) {
        saveAction
            .then(r => {
                this.container.success('Thành công');
                this.getSystemModules();
            })
            .catch(e => {
                this.container.error(
                    e.message ? e.message : e
                    , '', {
                        closeButton: true,
                    });
            });
    }
    delModule = (data) => () => {
        if (window.confirm(`Bạn muốn xóa module ${data.moduleName} với tất cả các phân quyền liên quan không?`)) {
            ApiEndpointService.removeModule(data)
                .then(r => {
                    this.container.warning('Xóa thành công!');
                    this.getSystemModules();
                })
                .catch(e => {
                    this.container.error(
                        e.message ? e.message : e
                        , '', {
                            closeButton: true,
                        });
                });
        }
    }

    render() {
        const { systemModuleGroups, systemModules } = this.state;
        let tabs = [];
        let contents = [];
        systemModuleGroups.forEach((g, index) => {

            tabs.push(getModulesTab(g, index == 0, this));

            let modules = systemModules.filter(m => m.moduleGroupId === g.moduleGroupId);

            contents.push(getModulesContent(g.moduleGroupId, modules, index == 0, this));
        })


        function getModulesTab(group, isActive, $this) {

            let stA = {
                padding: 5,
                margin: 3
            }

            let stp = {
                position: 'relative'
            }

            let st = {
                position: 'absolute',
                right: 20,
                top: 10
            }
            
            return (
                <li key={group.moduleGroupId} className="nav-item" style={stp} >
                    <a 
                        className={`nav-link ${isActive ? 'active' : ''}`}
                        data-module-groupid={group.moduleGroupId}
                        id={`v-modules-tab-${group.moduleGroupId}`}
                        data-toggle="pill"
                        href={`#v-modules-table-${group.moduleGroupId}`}
                        role="tab"
                        aria-controls={`v-modules-table-${group.moduleGroupId}`}
                        aria-selected={`${isActive ? 'true' : ''}`}
                    >
                        {group.moduleGroupId}. {group.moduleGroupName}
                    </a>
                    <div style={st}>
                        <a style={stA} className="text-warning" href='javascript:void(0)' onClick={$this.openEditGroup(group)}>Edit</a>
                        <a style={stA} className="text-danger" href='javascript:void(0)' onClick={$this.delGroup(group)}>Del</a>
                    </div>
                </li>
            );
        }

        function getModulesContent(groupId, modules, isActive, $this) {

            let stA = {
                padding: 5,
                margin: 3
            }

            return (
                <div key={groupId} className={`tab-pane fade ${(isActive ? ' show active' : '')}`}
                    id={`v-modules-table-${groupId}`}
                    role="tabpanel"
                    aria-labelledby={`v-modules-tab-${groupId}`}
                >
                    <table className="table table-sm">
                        <thead>
                            <tr>
                                <th scope="col">#</th>
                                <th scope="col">Name</th>
                                <th scope="col">Description</th>
                                <th scope="col"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                modules.map(m => (
                                    <tr key={m.moduleId}>
                                        <th scope="row">{m.moduleId}</th>
                                        <td>
                                            <NavLink tag={Link} to={`/module-apis-mapping/${m.moduleId}`}>
                                                {m.moduleName}
                                            </NavLink>
                                        </td>
                                        <td>{m.description}</td>
                                        <td>
                                            <a style={stA} className="text-warning" href='javascript:void(0)' onClick={$this.openEditModule(m)}>Edit</a>
                                            <a style={stA} className="text-danger" href='javascript:void(0)' onClick={$this.delModule(m)}>Del</a>
                                        </td>
                                    </tr>
                                ))
                            }

                        </tbody>
                    </table>
                </div>
            )
        }

        let st = {
            margin: 5
        }
        return (

            <div>
                <h4>Danh sách modules của hệ thống</h4>

                <ToastContainer ref={ref => this.container = ref}
                    className="toast-top-right" />

                <div className="row">
                    <div className="col-3">
                        <ul ref="tabGroups" className="nav flex-column nav-pills" id="v-pills-tab" role="tablist" aria-orientation="vertical">
                            {tabs}
                        </ul>
                        <div>
                            <a style={st} className="btn btn-success btn-sm" onClick={this.openEditGroup()}>Add</a>
                        </div>
                    </div>
                    <div className="col-9">
                        <div className="tab-content" id="v-pills-tabContent">
                            {contents}
                        </div>
                        <div>
                            <a style={st} className="btn btn-success btn-sm" onClick={this.openEditModule()}>Add</a>
                        </div>
                    </div>
                </div>
                <ModalEditModuleGroup ref="modalEditModuleGroup" onSave={this.saveModuleGroup} />
                <ModalEditModule ref="modalEditModule" onSave={this.saveModule}/>
            </div>
        );
    }
}