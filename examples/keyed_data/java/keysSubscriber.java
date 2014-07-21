
/* keysSubscriber.java

   A publication of data of type keys

   This file is derived from code automatically generated by the rtiddsgen 
   command:

   rtiddsgen -language java -example <arch> .idl

   Example publication of type keys automatically generated by 
   'rtiddsgen' To test them follow these steps:

   (1) Compile this file and the example subscription.

   (2) Start the subscription on the same domain used for with the command
       java keysSubscriber <domain_id> <sample_count>

   (3) Start the publication with the command
       java keysPublisher <domain_id> <sample_count>

   (4) [Optional] Specify the list of discovery initial peers and 
       multicast receive addresses via an environment variable or a file 
       (in the current working directory) called NDDS_DISCOVERY_PEERS. 
       
   You can run any number of publishers and subscribers programs, and can 
   add and remove them dynamically from the domain.
              
                                   
   Example:
        
       To run the example application on domain <domain_id>:
            
       Ensure that $(NDDSHOME)/lib/<arch> is on the dynamic library path for
       Java.                       
       
        On UNIX systems: 
             add $(NDDSHOME)/lib/<arch> to the 'LD_LIBRARY_PATH' environment
             variable
                                         
        On Windows systems:
             add %NDDSHOME%\lib\<arch> to the 'Path' environment variable
                        

       Run the Java applications:
       
        java -Djava.ext.dirs=$NDDSHOME/class keysPublisher <domain_id>

        java -Djava.ext.dirs=$NDDSHOME/class keysSubscriber <domain_id>  
       
       
modification history
------------ -------   
*/

import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.Arrays;

import com.rti.dds.domain.*;
import com.rti.dds.infrastructure.*;
import com.rti.dds.subscription.*;
import com.rti.dds.topic.*;
import com.rti.ndds.config.*;

// ===========================================================================

public class keysSubscriber {
    // -----------------------------------------------------------------------
    // Public Methods
    // -----------------------------------------------------------------------
    
    public static void main(String[] args) {
        // --- Get domain ID --- //
        int domainId = 0;
        if (args.length >= 1) {
            domainId = Integer.valueOf(args[0]).intValue();
        }
        
        // -- Get max loop count; 0 means infinite loop --- //
        int sampleCount = 0;
        if (args.length >= 2) {
            sampleCount = Integer.valueOf(args[1]).intValue();
        }
        
        
        /* Uncomment this to turn on additional logging
        Logger.get_instance().set_verbosity_by_category(
            LogCategory.NDDS_CONFIG_LOG_CATEGORY_API,
            LogVerbosity.NDDS_CONFIG_LOG_VERBOSITY_STATUS_ALL);
        */
        
        // --- Run --- //
        subscriberMain(domainId, sampleCount);
    }
    
    
    
    // -----------------------------------------------------------------------
    // Private Methods
    // -----------------------------------------------------------------------
    
    // --- Constructors: -----------------------------------------------------
    
    private keysSubscriber() {
        super();
    }
    
    
    // -----------------------------------------------------------------------
    
    private static void subscriberMain(int domainId, int sampleCount) {

        DomainParticipant participant = null;
        Subscriber subscriber = null;
        Topic topic = null;
        DataReaderListener listener = null;
        keysDataReader reader = null;

        try {

            // --- Create participant --- //
    
            /* To customize participant QoS, use
               the configuration file
               USER_QOS_PROFILES.xml */
    
            participant = DomainParticipantFactory.TheParticipantFactory.
                create_participant(
                    domainId, DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                    null /* listener */, StatusKind.STATUS_MASK_NONE);
            if (participant == null) {
                System.err.println("create_participant error\n");
                return;
            }                         

            // --- Create subscriber --- //
    
            /* To customize subscriber QoS, use
               the configuration file USER_QOS_PROFILES.xml */
    
            subscriber = participant.create_subscriber(
                DomainParticipant.SUBSCRIBER_QOS_DEFAULT, null /* listener */,
                StatusKind.STATUS_MASK_NONE);
            if (subscriber == null) {
                System.err.println("create_subscriber error\n");
                return;
            }     
                
            // --- Create topic --- //
        
            /* Register type before creating topic */
            String typeName = keysTypeSupport.get_type_name(); 
            keysTypeSupport.register_type(participant, typeName);
    
            /* To customize topic QoS, use
               the configuration file USER_QOS_PROFILES.xml */
    
            topic = participant.create_topic(
                "Example keys",
                typeName, DomainParticipant.TOPIC_QOS_DEFAULT,
                null /* listener */, StatusKind.STATUS_MASK_NONE);
            if (topic == null) {
                System.err.println("create_topic error\n");
                return;
            }                     
        
            // --- Create reader --- //

            listener = new keysListener();
    
            /* To customize data reader QoS, use
               the configuration file USER_QOS_PROFILES.xml */
    
            reader = (keysDataReader)
                subscriber.create_datareader(
                    topic, Subscriber.DATAREADER_QOS_DEFAULT, listener,
                    StatusKind.STATUS_MASK_ALL);
            if (reader == null) {
                System.err.println("create_datareader error\n");
                return;
            }                         
        
            // --- Wait for data --- //

            final long receivePeriodSec = 1;

            for (int count = 0;
                 (sampleCount == 0) || (count < sampleCount);
                 ++count) {
                //System.out.println("keys subscriber sleeping for " + receivePeriodSec + " sec...");
                try {
                    Thread.sleep(receivePeriodSec * 1000);  // in millisec
                } catch (InterruptedException ix) {
                    System.err.println("INTERRUPTED");
                    break;
                }
            }
        } finally {

            // --- Shutdown --- //

            if(participant != null) {
                participant.delete_contained_entities();

                DomainParticipantFactory.TheParticipantFactory.
                    delete_participant(participant);
            }
            /* RTI Connext provides the finalize_instance()
               method for users who want to release memory used by the
               participant factory singleton. Uncomment the following block of
               code for clean destruction of the participant factory
               singleton. */
            //DomainParticipantFactory.finalize_instance();
        }
    }
    
    // -----------------------------------------------------------------------
    // Private Types
    // -----------------------------------------------------------------------
    
    // =======================================================================
    
    private static class keysListener extends DataReaderAdapter {
            
        keysSeq _dataSeq = new keysSeq();
        SampleInfoSeq _infoSeq = new SampleInfoSeq();

        public void on_data_available(DataReader reader) {
            keysDataReader keysReader =
                (keysDataReader)reader;
            
            try {
                keysReader.take(
                    _dataSeq, _infoSeq,
                    ResourceLimitsQosPolicy.LENGTH_UNLIMITED,
                    SampleStateKind.ANY_SAMPLE_STATE,
                    ViewStateKind.ANY_VIEW_STATE,
                    InstanceStateKind.ANY_INSTANCE_STATE);

                for(int i = 0; i < _dataSeq.size(); ++i) {
                    SampleInfo info = (SampleInfo)_infoSeq.get(i);

                    /* Start changes for Keyed_Data */
                    /* We first check if the sample includes valid data */
                    if (info.valid_data) {
                        keys sample = (keys)_dataSeq.get(i);
                        if (info.view_state == ViewStateKind.NEW_VIEW_STATE) {
                            System.out.print("Found new instance; code = " + sample.code + "\n");
                        }
                        System.out.print("Instance " + sample.code + ": x: " + sample.x + ", y: " + sample.y + "\n");
                    } else {
                    	/* Since there is not valid data, it may include metadata */
                        keys dummy = new keys();
                        keysReader.get_key_value(dummy, info.instance_handle);
                        
                        /* Here we print a message if the instance state is ALIVE_NO_WRITERS or ALIVE_DISPOSED */
                        if (info.instance_state == InstanceStateKind.NOT_ALIVE_NO_WRITERS_INSTANCE_STATE) {
                            System.out.print("Instance " + dummy.code + " has no writers\n");
                        } else if (info.instance_state == InstanceStateKind.NOT_ALIVE_DISPOSED_INSTANCE_STATE) {
                            System.out.print("Instance " + dummy.code + " disposed\n");
                        }
                    }
                    /* End changes for Keyed_Data */
                }
            } catch (RETCODE_NO_DATA noData) {
                // No data to process
            } finally {
                keysReader.return_loan(_dataSeq, _infoSeq);
            }
        }
    }
}


        