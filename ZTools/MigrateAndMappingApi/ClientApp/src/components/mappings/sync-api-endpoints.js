import React, { Component } from 'react';
import { connect } from 'react-redux';

class SyncApiEndpoint extends Component {

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
    }



    render() {
        return (
            <button className="btn btn-primary" type="button" disabled={this.state.loading} onClick={this.executeAction(1)} >
                <span className={"spinner-border spinner-border-sm"} hidden={!this.state.loading} role="status" aria-hidden="true"></span>
                Đồng bộ data
            </button>
        );
    }
}

export default connect()(SyncApiEndpoint);
