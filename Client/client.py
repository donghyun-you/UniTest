#!/usr/bin/env python

#import asyncore, socket, json, sys, getopt
import json, sys, getopt, time
from twisted.internet import reactor, protocol, task

opts, args = getopt.getopt(sys.argv[1:],"ha:p:m:v:",["address=","port=","method=","argv="])

HOST="127.0.0.1"
PORT=7701
BUFFER_SIZE=0xff
METHOD="RunAllTest" # or RunTestOfType:NameOfType (ex: RunTestOfType:UniTest.Sample.TestBddSuccess) 
POLL_TIMEOUT=5
ARGV=None
EXITCODE=0
VARIABLE_TIMEOUT=30

for opt,arg in opts:
    if opt == '-h':
        print 'client.py --address <address> --port <port> --method <MethodNamespace.NestedNamespace.Class+NestedClass>'
        sys.exit(2)
    elif opt in ("-a","--address"):
        HOST=arg
    elif opt in ("-p","--port"):
        PORT=int(arg)
    elif opt in ("-m","--method"):
        METHOD=arg
    elif opt in ("-v","--argv"):
        ARGV=arg

print "UniTest Application Server Address: "+HOST
print "UniTest Application Server Port: "+str(PORT)
print "Test Method: "+METHOD
print "Test argv for method: "+str(ARGV)
COLORMAP={
        '<color=red>'       : '\033[1;31m',
        '<color=green>'     : '\033[1;32m',
        '<color=yellow>'    : '\033[1;33m',
        '<color=gray>'      : '\033[0;37m',
        '<color=blue>'      : '\033[0;34m',
        '<color=aqua>'      : '\033[1;34m',
        '<color=cyan>'      : '\033[1;34m',
        '<color=orange>'    : '\033[38;5;95;38;5;214m',
        '<color=purple>'    : '\033[0;35m',
        '<color=magenta>'   : '\033[0;35m',
        '<color=white>'     : '\033[1;37m',
        '</color>'          : '\033[0m'
         }

class UniTestClient(protocol.Protocol):

    recvBuffer          = None
    response            = None
    requestMessageType  = "STDIN"
    requestBody         = {"func":METHOD, "args":ARGV}
    lastUpdatedTime     = time.time()
    isConnected         = False

    class Response():
        length=-1
        messageType=None
        isReadingBody=False
        isReadingBodyComplete=False
        body=None

    def __init__(self):
        self.updateCall = task.LoopingCall(self.update)
        self.updateCall.start(1)

    def connectionMade(self):
        self.isConnected=True
        self.request(self.requestMessageType, self.requestBody)


    def connectionLost(self, reason):
        self.isConnected=False
        if self.updateCall.running:
            self.updateCall.stop()

    def dataReceived(self, data):
        self.lastUpdatedTime = time.time()
        self.consume_receive_packet(data)

    def updatedDeltaTime(self):
        return time.time() - self.lastUpdatedTime;

    def update(self):

        if self.updatedDeltaTime() > VARIABLE_TIMEOUT:

            if self.isConnected:
                print "variable timed out!"
            else:
                print "connection timed out!"

            EXITCODE=1
            reactor.stop()

    def request(self, message_type, body):
        bodyBuffer = json.dumps(body);
        message = 'Length: '+str(len(bodyBuffer))+'\n'+'MessageType: '+message_type+'\n\n'+bodyBuffer
        self.transport.write(message)

    def consume_receive_packet(self,received_buffer):

        if(self.recvBuffer is None):
            #print "Verbose> replacing receivedBuffer"
            self.recvBuffer = received_buffer
        else:
            #print "Verbose> adding"
            self.recvBuffer += received_buffer
        
        while(self.recvBuffer is not None and len(self.recvBuffer)>0):
            
            try: 
                index = self.recvBuffer.index('\n')+1
                
                self.process_receive_line(self.recvBuffer[:index])
                
                if(self.recvBuffer is not None):
                    self.recvBuffer = self.recvBuffer[index:]        
            
            except ValueError:
                self.process_receive_line(self.recvBuffer)
                self.recvBuffer = None
                break

    # NOTE(donghyun-you): line including line endings(\n)
    def process_receive_line(self,line):
        #print "Verbose> processing line: "+line
        if(self.response is None):
            self.response = self.Response()
        
        if(self.response.isReadingBody):

            if(self.response.body is None):
                self.response.body = line

            else:
                self.response.body += line

            if(len(self.response.body) >= self.response.length):
                
                body = self.response.body
                try:
                    body = json.loads(self.response.body) 
                except ValueError:
                    body = body

                self.receive_message(self.response.messageType,body)
                self.unset_processing_message()
            #done

        else:
            if(line.startswith("Length: ")):
                lengthStr=line[(len("Length: ")):].strip()
                self.response.length = int(lengthStr)
                #print "Verbose> message body length recognized: "+lengthStr

            elif(line.startswith("MessageType: ")):
                messageTypeStr=line[len("MessageType: "):].strip()
                self.response.messageType = messageTypeStr
                #print "Verbose> message type recognized: "+messageTypeStr

            elif(line == "\n"):
                if(self.response.length >= 0 and self.response.messageType is not None):
                    self.response.isReadingBody = True

                else:
                    #dropping message 
                    #print "Verbose> dropping message. invalid message received"
                    self.unset_processing_message()
                    
    def receive_message(self,message_type,body):
        global EXITCODE

        #print "Verbose> receiving message"
        #print "MessageType: "+message_type
        #print "Body/"
        #print body
        if(message_type == "STDOUT"):
            sys.stdout.write(self.replace_color_tags(body))
            sys.stdout.write("\n")

        elif(message_type == "STDERR"):
            sys.stderr.write(self.replace_color_tags(body))
            sys.stderr.write("\n")

        elif(message_type == "EXIT"):
            #print "Verbose> exiting by server signal: "+str(body)
            EXITCODE=int(body)
            reactor.stop()

        else:
            print message_type
            print body

    def replace_color_tags(self,src):
        for unityColorTag,shellColorCode in COLORMAP.iteritems():
            src = src.replace(unityColorTag,shellColorCode)

        return src    

    def unset_processing_message(self):
        self.recvBuffer = None
        self.response = None

class UniTestClientFactory(protocol.ClientFactory):
    protocol = UniTestClient

    def clientConnectionFailed(self, connector, reason):
        print "Connection failed"
        if reactor.running:
            reactor.stop()

    def clientConnectionLost(self, connector, reason):
        print "Connection lost"
        
        if reactor.running:
            reactor.stop()

def main():
    factory = UniTestClientFactory()
    reactor.connectTCP(HOST, PORT, factory)
    reactor.run()
    print "Exiting with: "+str(EXITCODE)
    sys.exit(EXITCODE)

if __name__ == '__main__':
    main()
