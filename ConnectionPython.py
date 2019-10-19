import socketserver
import socket

class MyTCPHandler(socketserver.BaseRequestHandler):

    def handle(self):
        self.data = self.request.recv(1024).strip()
        print (self.data)
        self.request.sendall(self.data.upper())

if __name__ == "__main__":
    HOST, PORT = socket.gethostname(), 9182

    server = socketserver.TCPServer((HOST, PORT), MyTCPHandler)
    server.serve_forever()
