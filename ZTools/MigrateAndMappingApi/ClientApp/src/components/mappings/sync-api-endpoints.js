import React, { Component } from 'react';
import { connect } from 'react-redux';
import ApiEndpointService from './../../services/ApiEndpointService';
import { ToastContainer } from 'react-toastr'
import { ToastMessage } from "react-toastr";

import './../../css/toastr.min.css';
import './../../css/animate.min.css';

class SyncApiEndpoint extends Component {
    container;

    constructor(props) {
        super(props);
        this.state = {
            loading: false
        }

    }

    componentDidMount() {

    }

    executeAction = (num) => () => {
        console.log(num);
        this.setState({
            loading: true
        })
        ApiEndpointService.syncApiEndpoint()
            .then(() => {
                this.setState({
                    loading: false
                })
            })
            .catch((e) => {
                this.setState({
                    loading: false
                });
                
                this.container.error(
                    e.message
                    , '',{
                        closeButton: true,
                    });
            });
    }



    render() {
        
        return (
            <div>
                <ToastContainer ref={ref => this.container = ref} 
                    className="toast-top-right"/>
                <button className="btn btn-primary" type="button" disabled={this.state.loading} onClick={this.executeAction(1)} >
                    <span className={"spinner-border spinner-border-sm"} hidden={!this.state.loading} role="status" aria-hidden="true"></span>
                    Đồng bộ data
                </button>
            </div>
        );
    }
}

export default connect()(SyncApiEndpoint);
