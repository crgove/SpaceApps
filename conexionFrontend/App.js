/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 *
 * @format
 * @flow
 */

import React, {Component} from 'react';
import {
  StyleSheet,
  View,
  Text,
  TextInput,
  Button,
} from 'react-native';
import * as Sensors from 'react-native-sensors'


// The class we will use to connect our frontEnd with our backend
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
    fetch('https://apispaceapps.azurewebsites.net/api/dron/status', {
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


const accelerometer = Sensors["accelerometer"]

class App extends Component {

  textIp = ""
  textPort = ""
  connection = new Connection


  constructor(props){
    super(props)
    
    this.state = {
      connected: false, 
      the: 0.0,
      phi: 0.0,
      x: 0,
      y: 0,
      z: 0,
      timer: null
    }
  }

  componentWillUnmount(){
    if(this.state.connected){
      clearInterval(this.state.timer)

      this.state.subscription.unsubscribe();
      this.setState({ subscription: null });
    }
  }

  render(){
    if(this.state.connected) {
      return(
        <View style = {styles.body}>
          <View>
            <Text>Simulation variables</Text>
            <Text>The: {this.state.the} phi: {this.state.phi}</Text>
          </View>
          <View>
            <Text>Device variables</Text>
            <Text>bet: {this.state.x.toFixed(2)} alp: {this.state.y.toFixed(2)}</Text>
          </View>
        </View>
      )
    }

    return (
        <View style = {styles.body}>
          <Text>IP:</Text>
          <TextInput onChangeText={text => this.textIp = text}/>
          <Text>PORT:</Text>
          <TextInput onChangeText={text => this.textPort = text}/>
          <Button title="Connect Me" onPress={ () => {
            // Making the connectión with the dron and reseting the simulation
            this.connection.DronConnection(this.textIp, this.textPort)
            
            // starting the accelerometer sensor
            const subscription = accelerometer.subscribe(values => {
              this.setState({ ...values });
            });
            this.setState({ subscription });
            
            this.setState({
              connected: true,
              // every 100ms we will refresh the state of the dron
              timer: setInterval(() => {
                this.connection.DronRefreshState(this.state.y, this.state.x)
        
                this.setState({
                  the: this.connection.the,
                  phi: this.connection.phi,
                })
              }, 100)})
          }}/>
        </View>
    )
  }
}

const styles = StyleSheet.create({
  body: {
    justifyContent: "center",
    alignItems: "center",
    flex: 1
  }
})

export default App;
