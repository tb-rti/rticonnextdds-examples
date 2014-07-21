using System;
using System.Collections.Generic;
using System.Text;
/* partitions_publisher.cs

   A publication of data of type partitions

   This file is derived from code automatically generated by the rtiddsgen 
   command:

   rtiddsgen -language C# -example <arch> partitions.idl

   Example publication of type partitions automatically generated by 
   'rtiddsgen'. To test them follow these steps:

   (1) Compile this file and the example subscription.

   (2) Start the subscription with the command
       objs\<arch>\partitions_subscriber <domain_id> <sample_count>
                
   (3) Start the publication with the command
       objs\<arch>\partitions_publisher <domain_id> <sample_count>

   (4) [Optional] Specify the list of discovery initial peers and 
       multicast receive addresses via an environment variable or a file 
       (in the current working directory) called NDDS_DISCOVERY_PEERS. 

   You can run any number of publishers and subscribers programs, and can 
   add and remove them dynamically from the domain.


   Example:

       To run the example application on domain <domain_id>:

       bin\<Debug|Release>\partitions_publisher <domain_id> <sample_count>
       bin\<Debug|Release>\partitions_subscriber <domain_id> <sample_count>

       
modification history
------------ -------       
*/

public class partitionsPublisher
{

    public static void Main(string[] args)
    {

        // --- Get domain ID --- //
        int domain_id = 0;
        if (args.Length >= 1)
        {
            domain_id = Int32.Parse(args[0]);
        }

        // --- Get max loop count; 0 means infinite loop  --- //
        int sample_count = 0;
        if (args.Length >= 2)
        {
            sample_count = Int32.Parse(args[1]);
        }

        /* Uncomment this to turn on additional logging
        NDDS.ConfigLogger.get_instance().set_verbosity_by_category(
            NDDS.LogCategory.NDDS_CONFIG_LOG_CATEGORY_API, 
            NDDS.LogVerbosity.NDDS_CONFIG_LOG_VERBOSITY_STATUS_ALL);
        */

        // --- Run --- //
        try
        {
            partitionsPublisher.publish(
                domain_id, sample_count);
        }
        catch (DDS.Exception)
        {
            Console.WriteLine("error in publisher");
        }
    }

    static void publish(int domain_id, int sample_count)
    {

        // --- Create participant --- //

        /* To customize participant QoS, use 
           the configuration file USER_QOS_PROFILES.xml */
        DDS.DomainParticipant participant =
            DDS.DomainParticipantFactory.get_instance().create_participant(
                domain_id,
                DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
        if (participant == null)
        {
            shutdown(participant);
            throw new ApplicationException("create_participant error");
        }

        // --- Create publisher --- //

        DDS.PublisherQos publisher_qos = new DDS.PublisherQos();
        participant.get_default_publisher_qos(publisher_qos);


        /* If you want to change the Partition name programmatically rather than
         * using the XML, you will need to add the following lines to your code
         * and comment out the create_publisher() call bellow.
         */
        /*
        String[] partitions = { "ABC", "foo" };

        publisher_qos.partition.name.ensure_length(2, 2);
        publisher_qos.partition.name.from_array(partitions);
        DDS.Publisher publisher = participant.create_publisher(
                                publisher_qos,
                                null,
                                DDS.StatusMask.STATUS_MASK_NONE);          
        
        */
        DDS.Publisher publisher = participant.create_publisher(
        DDS.DomainParticipant.PUBLISHER_QOS_DEFAULT,
        null,
        DDS.StatusMask.STATUS_MASK_NONE);
        if (publisher == null)
        {
            shutdown(participant);
            throw new ApplicationException("create_publisher error");
        }

        Console.WriteLine("Setting partition to '{0}', '{1}'...",
                          publisher_qos.partition.name.get_at(0),
                          publisher_qos.partition.name.get_at(1));

        // --- Create topic --- //

        /* Register type before creating topic */
        System.String type_name = partitionsTypeSupport.get_type_name();
        try
        {
            partitionsTypeSupport.register_type(
                participant, type_name);
        }
        catch (DDS.Exception e)
        {
            Console.WriteLine("register_type error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* To customize topic QoS, use 
           the configuration file USER_QOS_PROFILES.xml */
        DDS.Topic topic = participant.create_topic(
            "Example partitions",
            type_name,
            DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (topic == null)
        {
            shutdown(participant);
            throw new ApplicationException("create_topic error");
        }

        // --- Create writer --- //

        /* In this example we set a Reliable datawriter, with Transient Local 
         * durability. By default we set up these QoS settings via XML. If you
         * want to to it programmatically, use the following code, and comment out
         * the create_datawriter call bellow.
         */

        /*
        DDS.DataWriterQos writerQos = new DDS.DataWriterQos();
        publisher.get_default_datawriter_qos(writerQos);

        writerQos.reliability.kind = DDS.ReliabilityQosPolicyKind.RELIABLE_RELIABILITY_QOS;
        writerQos.history.depth = 3;
        writerQos.history.kind = DDS.HistoryQosPolicyKind.KEEP_LAST_HISTORY_QOS;
        writerQos.durability.kind = DDS.DurabilityQosPolicyKind.TRANSIENT_LOCAL_DURABILITY_QOS;

        DDS.DataWriter writer = publisher.create_datawriter(
            topic,
            writerQos,
            null,
            DDS.StatusMask.STATUS_MASK_NONE);
        */

        DDS.DataWriter writer = publisher.create_datawriter(
            topic,
            DDS.Publisher.DATAWRITER_QOS_DEFAULT,
            null,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (writer == null)
        {
            shutdown(participant);
            throw new ApplicationException("create_datawriter error");
        }
        partitionsDataWriter partitions_writer =
            (partitionsDataWriter)writer;

        // --- Write --- //

        /* Create data sample for writing */
        partitions instance = partitionsTypeSupport.create_data();
        if (instance == null)
        {
            shutdown(participant);
            throw new ApplicationException(
                "partitionsTypeSupport.create_data error");
        }

        /* For a data type that has a key, if the same instance is going to be
           written multiple times, initialize the key here
           and register the keyed instance prior to writing */
        DDS.InstanceHandle_t instance_handle = DDS.InstanceHandle_t.HANDLE_NIL;
        /*
        instance_handle = partitions_writer.register_instance(instance);
        */

        /* Main loop */
        const System.Int32 send_period = 1000; // 1 second
        for (int count = 0;
             (sample_count == 0) || (count < sample_count);
             ++count)
        {
            Console.WriteLine("Writing partitions, count {0}", count);

            /* Modify the data to be sent here */
            instance.x = count;

            try
            {
                partitions_writer.write(instance, ref instance_handle);
            }
            catch (DDS.Exception e)
            {
                Console.WriteLine("write error {0}", e);
            }

            if ((count + 1) % 25 == 0)
            {
                // Matches "ABC" -- name[1] here can match name[0] there, 
                // as long as there is some overlapping name
                publisher_qos.partition.name.set_at(0, "zzz");
                publisher_qos.partition.name.set_at(1, "A*C");
                Console.WriteLine("Setting partition to '{0}', '{1}'...",
                                  publisher_qos.partition.name.get_at(0),
                                  publisher_qos.partition.name.get_at(1));
                publisher.set_qos(publisher_qos);
            }
            else if ((count + 1) % 20 == 0)
            {
                // Strings that are regular expressions aren't tested for
                // literal matches, so this won't match "X*Z"
                publisher_qos.partition.name.set_at(0, "X*Z");
                Console.WriteLine("Setting partition to '{0}', '{1}'...",
                                  publisher_qos.partition.name.get_at(0),
                                  publisher_qos.partition.name.get_at(1));
                publisher.set_qos(publisher_qos);
            }
            else if ((count + 1) % 15 == 0)
            {
                // Matches "ABC"
                publisher_qos.partition.name.set_at(0, "A?C");
                Console.WriteLine("Setting partition to '{0}', '{1}'...",
                                  publisher_qos.partition.name.get_at(0),
                                  publisher_qos.partition.name.get_at(1));
                publisher.set_qos(publisher_qos);
            }
            else if ((count + 1) % 10 == 0)
            {
                // Matches "ABC"
                publisher_qos.partition.name.set_at(0, "A*");
                Console.WriteLine("Setting partition to '{0}', '{1}'...",
                                  publisher_qos.partition.name.get_at(0),
                                  publisher_qos.partition.name.get_at(1));
                publisher.set_qos(publisher_qos);
            }
            else if ((count + 1) % 5 == 0)
            {
                // No literal match for "bar"
                publisher_qos.partition.name.set_at(0, "bar");
                Console.WriteLine("Setting partition to '{0}', '{1}'...",
                                  publisher_qos.partition.name.get_at(0),
                                  publisher_qos.partition.name.get_at(1));
                publisher.set_qos(publisher_qos);
            }


            System.Threading.Thread.Sleep(send_period);
        }

        /*
        try {
            partitions_writer.unregister_instance(
                instance, ref instance_handle);
        } catch(DDS.Exception e) {
            Console.WriteLine("unregister instance error: {0}", e);
        }
        */

        // --- Shutdown --- //

        /* Delete data sample */
        try
        {
            partitionsTypeSupport.delete_data(instance);
        }
        catch (DDS.Exception e)
        {
            Console.WriteLine(
                "partitionsTypeSupport.delete_data error: {0}", e);
        }

        /* Delete all entities */
        shutdown(participant);
    }

    static void shutdown(
        DDS.DomainParticipant participant)
    {

        /* Delete all entities */

        if (participant != null)
        {
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(
                ref participant);
        }

        /* RTI Connext provides finalize_instance() method on
           domain participant factory for people who want to release memory
           used by the participant factory. Uncomment the following block of
           code for clean destruction of the singleton. */
        /*
        try {
            DDS.DomainParticipantFactory.finalize_instance();
        } catch (DDS.Exception e) {
            Console.WriteLine("finalize_instance error: {0}", e);
            throw e;
        }
        */
    }
}

