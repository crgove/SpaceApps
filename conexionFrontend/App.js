/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 *
 * @format
 * @flow
 */

import React, {Component} from 'react';
import {
  SafeAreaView,
  StyleSheet,
  ScrollView,
  View,
  Text,
  StatusBar,
  TextInput,
  Button,
} from 'react-native';

class Connection {
  the = 0
  phi = 0

  DronConnection(ip, port) {
    fetch('https://apispaceapps.azurewebsites.net/api/dron/dir', {
      method: 'PUT',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        Ip: ip,
        Port: port,
      }),
    })
    .catch((error) => {
        console.error(error);
    });
  }

  DronRefreshState(alpha, beta){
    fetch('https://192.168.43.219:44345/api/dron/status', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        Alp: alpha,
        Bet: beta,
      }),
    }) 
    .then(response => response.json())
    .then((responseJson) => {
        this.the = responseJson.the
        this.phi = responseJson.phi
    })
    .catch((error) => {
        console.error(error);
    });
  }
}


class App extends Component {
  // Variables de la clase
  textIp = ""
  textPort = ""
  connection = new Connection

  constructor(props){
    super(props)
    
    this.state = {
      connected: false, 
      the: 0.0,
      phi: 0.0,
      alp: 0.0,
      bet: 0.0,
      timer: null
    }
  }

  componentWillUnmount(){
    clearInterval(this.state.timer)
  }

  render(){
    if(this.state.connected) {
      return(
        <View>
          <Text>The = {this.state.the}</Text>
          <Text>phi = {this.state.phi}</Text>
        </View>
      )
    }

    return (
        <View>
          <Text>IP:</Text>
          <TextInput onChangeText={text => this.textIp = text}/>
          <Text>PORT:</Text>
          <TextInput onChangeText={text => this.textPort = text}/>
          <Button title="Connect Me" onPress={() => {
            this.connection.DronConnection(this.textIp, this.textPort)
            
            this.setState({
              connected: true,
              timer: setInterval(() => {
                await this.connection.DronRefreshState(this.state.alp, this.state.bet)
        
                this.setState({
                  the: this.connection.the,
                  phi: this.connection.phi
                })
              }, 100)
            })
          }}/>
        </View>
    )
  }
}



export default App;
