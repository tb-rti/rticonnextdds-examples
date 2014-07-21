using System;
using System.Collections.Generic;
using System.Text;
/* waitsets_subscriber.cs

   A subscription example

   This file is derived from code automatically generated by the rtiddsgen 
   command:

   rtiddsgen -language C# -example <arch> waitsets.idl

   Example subscription of type waitsets automatically generated by 
   'rtiddsgen'. To test them, follow these steps:

   (1) Compile this file and the example publication.

   (2) Start the subscription with the command
       objs\<arch>\waitsets_subscriber <domain_id> <sample_count>

   (3) Start the publication with the command
       objs\<arch>\waitsets_publisher <domain_id> <sample_count>

   (4) [Optional] Specify the list of discovery initial peers and 
       multicast receive addresses via an environment variable or a file 
       (in the current working directory) called NDDS_DISCOVERY_PEERS. 

   You can run any number of publishers and subscribers programs, and can 
   add and remove them dynamically from the domain.
                                   
   Example:
        
       To run the example application on domain <domain_id>:
                          
       bin\<Debug|Release>\waitsets_publisher <domain_id> <sample_count>  
       bin\<Debug|Release>\waitsets_subscriber <domain_id> <sample_count>
              
       
modification history
------------ -------
*/

public class waitsetsSubscriber {

    /* We don't need to use listeners as we are going to use Waitsets and 
     * Conditions so we have removed the auto generated code for listeners here 
     */

    public static void Main( string[] args ) {

        // --- Get domain ID --- //
        int domain_id = 0;
        if (args.Length >= 1) {
            domain_id = Int32.Parse(args[0]);
        }

        // --- Get max loop count; 0 means infinite loop  --- //
        int sample_count = 0;
        if (args.Length >= 2) {
            sample_count = Int32.Parse(args[1]);
        }

        /* Uncomment this to turn on additional logging
        NDDS.ConfigLogger.get_instance().set_verbosity_by_category(
            NDDS.LogCategory.NDDS_CONFIG_LOG_CATEGORY_API, 
            NDDS.LogVerbosity.NDDS_CONFIG_LOG_VERBOSITY_STATUS_ALL);
        */

        // --- Run --- //
        try {
            waitsetsSubscriber.subscribe(
                domain_id, sample_count);
        } catch (DDS.Exception) {
            Console.WriteLine("error in subscriber");
        }
    }

    static void subscribe( int domain_id, int sample_count ) {

        // --- Create participant --- //

        /* To customize the participant QoS, use 
           the configuration file USER_QOS_PROFILES.xml */
        DDS.DomainParticipant participant =
            DDS.DomainParticipantFactory.get_instance().create_participant(
                domain_id,
                DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
        if (participant == null) {
            shutdown(participant);
            throw new ApplicationException("create_participant error");
        }

        // --- Create subscriber --- //

        /* To customize the subscriber QoS, use 
           the configuration file USER_QOS_PROFILES.xml */
        DDS.Subscriber subscriber = participant.create_subscriber(
            DDS.DomainParticipant.SUBSCRIBER_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (subscriber == null) {
            shutdown(participant);
            throw new ApplicationException("create_subscriber error");
        }

        // --- Create topic --- //

        /* Register the type before creating the topic */
        System.String type_name = waitsetsTypeSupport.get_type_name();
        try {
            waitsetsTypeSupport.register_type(
                participant, type_name);
        } catch (DDS.Exception e) {
            Console.WriteLine("register_type error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* To customize the topic QoS, use 
           the configuration file USER_QOS_PROFILES.xml */
        DDS.Topic topic = participant.create_topic(
            "Example waitsets",
            type_name,
            DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (topic == null) {
            shutdown(participant);
            throw new ApplicationException("create_topic error");
        }

        // --- Create reader --- //

        /* To customize the data reader QoS, use 
           the configuration file USER_QOS_PROFILES.xml */
        DDS.DataReader reader = subscriber.create_datareader(
            topic,
            DDS.Subscriber.DATAREADER_QOS_DEFAULT,
            null,
            DDS.StatusMask.STATUS_MASK_ALL);
        if (reader == null) {
            shutdown(participant);
            throw new ApplicationException("create_datareader error");
        }

        /* If you want to change the DataReader's QoS programmatically rather 
         * than using the XML file, you will need to add the following lines to 
         * your code and comment out the create_datareader call above.
         *
         * In this case, we reduce the liveliness timeout period to trigger the
         * StatusCondition DDS.StatusKind.LIVELINESS_CHANGED_STATUS
         */
        /*
        DDS.DataReaderQos datawriter_qos = new DDS.DataReaderQos();
        try {
            subscriber.get_default_datareader_qos(datawriter_qos);
        } catch (DDS.Exception e) {
            Console.WriteLine("get_default_datareader_qos error {0}", e);
            shutdown(participant);
            throw e;
        }
        datawriter_qos.liveliness.lease_duration.sec = 2;
        datawriter_qos.liveliness.lease_duration.nanosec = 0;

        reader = subscriber.create_datareader(topic, datawriter_qos,
            null, DDS.StatusMask.STATUS_MASK_NONE);
        if (reader == null) {
            shutdown(participant);
            throw new ApplicationException("create_datawriter_qos error");
        }
        */

        /* Create read condition
         * ---------------------
         * Note that the Read Conditions are dependent on both incoming
         * data as well as sample state. Thus, this method has more
         * overhead than adding a DDS.StatusKind.DATA_AVAILABLE_STATUS
         * StatusCondition. We show it here purely for reference
         */
        DDS.ReadCondition read_condition = reader.create_readcondition(
            DDS.SampleStateKind.NOT_READ_SAMPLE_STATE,
            DDS.ViewStateKind.ANY_VIEW_STATE,
            DDS.InstanceStateKind.ANY_INSTANCE_STATE);
        if (read_condition == null) {
            shutdown(participant);
            throw new ApplicationException("create_readcondition error");
        }

        /* Get status conditions
         * ---------------------
         * Each entity may have an attached Status Condition. To modify the
         * statuses we need to get the reader's Status Conditions first.
         */

        DDS.StatusCondition status_condition = reader.get_statuscondition();
        if (status_condition.get_entity() == null) {
            shutdown(participant);
            throw new ApplicationException("get_statuscondition error");
        }

        /* Set enabled statuses
         * --------------------
         * Now that we have the Status Condition, we are going to enable the
         * statuses we are interested in: 
         * DDS.StatusKind.SUBSCRIPTION_MATCHED_STATUS
         * and DDS.StatusKind.LIVELINESS_CHANGED_STATUS.
         */
        try {
            status_condition.set_enabled_statuses(
                DDS.StatusMask.STATUS_MASK_NONE |
                (DDS.StatusMask)DDS.StatusKind.SUBSCRIPTION_MATCHED_STATUS |
                (DDS.StatusMask)DDS.StatusKind.LIVELINESS_CHANGED_STATUS);
        } catch (DDS.Exception e) {
            Console.WriteLine("set_enabled_statuses error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* Create and attach conditions to the WaitSet
         * -------------------------------------------
         * Finally, we create the WaitSet and attach both the Read Conditions
         * and the Status Condition to it.
         */
        DDS.WaitSet waitset = new DDS.WaitSet();

        /* Attach Read Conditions */
        try {
            waitset.attach_condition(read_condition);
        } catch (DDS.Exception e) {
            Console.WriteLine("attach_read_condition error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* Attach Status Conditions */
        try {
            waitset.attach_condition(status_condition);
        } catch (DDS.Exception e) {
            Console.WriteLine("attach_status_condition error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* Narrow the reader into your specific data type */
        waitsetsDataReader waitsets_reader =
            (waitsetsDataReader)reader;


        /* Main loop */
        for (int count = 0; (sample_count == 0) || (count < sample_count); 
                ++count) {
            DDS.ConditionSeq active_conditions_seq = new DDS.ConditionSeq();
            DDS.Duration_t timeout;
            timeout.nanosec = (uint)500000000;
            timeout.sec = 1;

            /* wait() blocks execution of the thread until one or more attached
             * Conditions become true, or until a user-specified timeout expires
             */
            try {
                waitset.wait(active_conditions_seq, timeout);
            } catch (DDS.Retcode_Timeout) {
                Console.WriteLine("Wait timed out!! No conditions were " + 
                    "triggered.");
                continue;
            } catch (DDS.Exception e) {
                Console.WriteLine("wait error {0}", e);
                break;
            }

            /* Get the number of active conditions */
            int active_conditions = active_conditions_seq.length;
            Console.WriteLine("Got {0} active conditions", active_conditions);

            for (int i = 0; i < active_conditions; i++) {
                /* Now we compare the current condition with the Status
                 * Conditions and the Read Conditions previously defined. If
                 * they match, we print the condition that was triggered.*/

                /* Compare with Status Conditions */
                if (active_conditions_seq.get_at(i) == status_condition) {
                    /* Get the status changes so we can check which status
                     * condition triggered. */
                    DDS.StatusMask triggeredmask =
                        waitsets_reader.get_status_changes();

                    /* Liveliness changed */
                    DDS.StatusMask test = triggeredmask &
                        (DDS.StatusMask)DDS.StatusKind.
                            LIVELINESS_CHANGED_STATUS;
                    if (test != DDS.StatusMask.STATUS_MASK_NONE) {
                        DDS.LivelinessChangedStatus st = 
                            new DDS.LivelinessChangedStatus();
                        waitsets_reader.get_liveliness_changed_status(ref st);
                        Console.WriteLine("Liveliness changed => " + 
                            "Active writers = {0}", st.alive_count);
                    }

                    /* Subscription matched */
                    test = triggeredmask &
                        (DDS.StatusMask)DDS.StatusKind.
                            SUBSCRIPTION_MATCHED_STATUS;
                    if (test != DDS.StatusMask.STATUS_MASK_NONE) {
                        DDS.SubscriptionMatchedStatus st = 
                            new DDS.SubscriptionMatchedStatus();
                        waitsets_reader.get_subscription_matched_status(ref st);
                        Console.WriteLine("Subscription matched => " + 
                            "Cumulative matches = {0}", st.total_count);
                    }
                }

                /* Compare with Read Conditions */
                else if (active_conditions_seq.get_at(i) == read_condition) {
                    /* Current conditions match our conditions to read data, so
                     * we can read data just like we would do in any other
                     * example. */
                    waitsetsSeq data_seq = new waitsetsSeq();
                    DDS.SampleInfoSeq info_seq = new DDS.SampleInfoSeq();

                    /* You may want to call take_w_condition() or
                     * read_w_condition() on the Data Reader. This way you will
                     * use the same status masks that were set on the Read 
                     * Condition.
                     * This is just a suggestion, you can always use
                     * read() or take() like in any other example.
                     */
                    try {
                        waitsets_reader.take_w_condition(
                            data_seq, info_seq,
                            DDS.ResourceLimitsQosPolicy.LENGTH_UNLIMITED,
                            read_condition);
                    } catch (DDS.Exception e) {
                        Console.WriteLine("take error {0}", e);
                        break;
                    }

                    for (int j = 0; j < data_seq.length; ++j) {
                        if (!info_seq.get_at(j).valid_data) {
                            Console.WriteLine("Got metadata");
                            continue;
                        }
                        waitsetsTypeSupport.print_data(data_seq.get_at(i));
                    }
                    waitsets_reader.return_loan(data_seq, info_seq);
                }

            }

        }

        // --- Shutdown --- //

        /* Delete all entities */
        shutdown(participant);
    }


    static void shutdown(
        DDS.DomainParticipant participant ) {

        /* Delete all entities */

        if (participant != null) {
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(
                ref participant);
        }

        /* RTI Connext provides finalize_instance() method on
           domain participant factory for users who want to release memory
           used by the participant factory. Uncomment the following block of
           code for clean destruction of the singleton. */
        /*
        try {
            DDS.DomainParticipantFactory.finalize_instance();
        }
        catch(DDS.Exception e) {
            Console.WriteLine("finalize_instance error {0}", e);
            throw e;
        }
        */
    }
}


